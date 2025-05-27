using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StorkDorkMain.Models;
using StorkDorkMain.DAL.Abstract;
using System.Threading.Tasks;

namespace StorkDorkMain.Areas.Identity.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ISDUserRepository _sdUserRepository;
        private readonly IMilestoneRepository _milestoneRepository;

        public IndexModel(UserManager<IdentityUser> userManager, ISDUserRepository sdUserRepository, IMilestoneRepository milestoneRepository)
        {
            _userManager = userManager;
            _sdUserRepository = sdUserRepository;
            _milestoneRepository = milestoneRepository;
        }

        public string Email { get; set; }

        public int SightingsMade { get; set; }
        public int PhotosContributed { get; set; }
        public string MilestoneTier { get; set; }

        public string? ProfileImagePath { get; set; }

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public UserSettings UserSettings {get;set;}

        public class InputModel
        {
            public string DisplayName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            Email = user.Email;

            // Load your custom StorkDork user info
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);

            if (sdUser != null)
            {
                Input = new InputModel
                {
                    DisplayName = sdUser.DisplayName
                };

                // Get Sightings and Photos
                SightingsMade = await _milestoneRepository.GetSightingsMade(sdUser.Id);
                PhotosContributed = await _milestoneRepository.GetPhotosContributed(sdUser.Id);

                // Milestone Tier based on achievements
                int tier = _milestoneRepository.GetMilestoneTier(SightingsMade);
                MilestoneTier = GetTierName(tier);
                ProfileImagePath = sdUser?.ProfileImagePath;

                // UserSettings
                UserSettings = sdUser.UserSettings ?? new UserSettings { SdUserId = sdUser.Id };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUploadImageAsync()
        {
            if (ProfileImage == null || ProfileImage.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an image file.");
                return await OnGetAsync();
            }

            if (ProfileImage.Length > 2 * 1024 * 1024) // 2MB max
            {
                ModelState.AddModelError(string.Empty, "File size exceeds 2MB limit.");
                return await OnGetAsync();
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(ProfileImage.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "Invalid file format. Only JPG, JPEG, and PNG are allowed.");
                return await OnGetAsync();
            }

            var user = await _userManager.GetUserAsync(User);
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);

            if (sdUser == null)
            {
                return NotFound("User profile not found.");
            }

            // Delete old file if exists
            if (!string.IsNullOrEmpty(sdUser.ProfileImagePath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sdUser.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // Save new file
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine("wwwroot/images/profiles", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = System.IO.File.Create(filePath))
            {
                await ProfileImage.CopyToAsync(stream);
            }

            // Save path in DB
            sdUser.ProfileImagePath = $"/images/profiles/{fileName}";
            await _sdUserRepository.UpdateAsync(sdUser);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveImageAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);

            if (sdUser == null)
            {
                return NotFound("User profile not found.");
            }

            if (!string.IsNullOrEmpty(sdUser.ProfileImagePath))
            {
                var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", sdUser.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
                sdUser.ProfileImagePath = null;
                await _sdUserRepository.UpdateAsync(sdUser);
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveSettingsAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);

            if (sdUser == null)
            {
                return NotFound("User profile not found.");
            }

            // Update or create UserSettings
            if (sdUser.UserSettings == null)
            {
                sdUser.UserSettings = new UserSettings
                {
                    SdUserId = sdUser.Id,
                    AnonymousSightings = UserSettings.AnonymousSightings
                };
            }
            else
            {
                sdUser.UserSettings.AnonymousSightings = UserSettings.AnonymousSightings;
            }

            await _sdUserRepository.UpdateAsync(sdUser);

            // Optionally add a success message
            TempData["StatusMessage"] = "Settings updated successfully.";

            return RedirectToPage();
        }



        private string GetTierName(int tier)
        {
            return tier switch
            {
                1 => "Gold",
                2 => "Silver",
                3 => "Bronze",
                _ => "No Tier"
            };
        }
    }
}
