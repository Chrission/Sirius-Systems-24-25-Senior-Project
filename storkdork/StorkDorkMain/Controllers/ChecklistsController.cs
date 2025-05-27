using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StorkDorkMain.Data;
using StorkDorkMain.Models;
using Microsoft.AspNetCore.Authorization;
using StorkDorkMain.DAL.Abstract;
//
namespace StorkDorkMain.Controllers
{
    public class ChecklistsController : Controller
    {
        private readonly StorkDorkDbContext _context;
        private readonly ISDUserRepository _sdUserRepository;

        public ChecklistsController(StorkDorkDbContext context, ISDUserRepository sdUserRepository)
        {
            _context = context;
            _sdUserRepository = sdUserRepository;
        }

        public async Task<IActionResult> SearchBirds(string query)
        {
            var birds = await _context.Birds
                .Where(b => b.CommonName.Contains(query) || b.ScientificName.Contains(query))
                .Take(20)
                .Select(b => new
                    {
                        id = b.Id,
                        commonName = b.CommonName,
                        scientificName = b.ScientificName
                    })
                    .ToListAsync();
            return Json(birds);
        } 

          

        // GET: Checklists
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("User is not authenticated");
            }
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound("User not found");
            }

            //Get the checklsits for the logged in user
            var checklists = await _context.Checklists
                .Include(c => c.SdUser)
                .Include(c => c.ChecklistItems)
                    .ThenInclude(ci => ci.Bird)
                .Where(c => c.SdUserId == sdUser.Id)
                .ToListAsync();
        
            if (!checklists.Any())
            {
                ViewBag.NoChecklistsMessage = "No checklists found. Create one?";
            }

            ViewBag.UserName = sdUser.FirstName;
            return View(checklists);

        }
        

        // GET: Checklists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var checklist = await _context.Checklists
                .Include(c => c.SdUser)
                .Include(c => c.ChecklistItems)
                    .ThenInclude(ci => ci.Bird)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (checklist == null)
            {
                return NotFound();
            }

            return View(checklist);
        }


        // GET: Checklists/Create
        public async Task<IActionResult> Create()
        {
            // Fetch the details for the user
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound("User not found");
            }

            // Fetch all birds from the database to display in the form
            var birds = _context.Birds
            .Select(b => new SelectListItem {
                Value = b.Id.ToString(),
                Text  = b.CommonName
            })
            .ToList();
            ViewBag.Birds = new SelectList(birds, "Id", "CommonName"); // Pass birds to the view

            ViewBag.SelectedSdUserId = sdUser.Id;
            return View();
        }



        // POST: Checklists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChecklistName,SdUserId")] Checklist checklist, int[] selectedBirds)
        {
            // Get the current user's SdUser
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound("User not found");
            }

            checklist.SdUserId = sdUser.Id; 
            
            if (ModelState.IsValid)
            {
                _context.Add(checklist);
                await _context.SaveChangesAsync();
            
                if (selectedBirds != null)
                {
                    foreach (var birdId in selectedBirds)
                    {
                        var checklistItem = new ChecklistItem
                        {
                            ChecklistId = checklist.Id,
                            BirdId = birdId,
                            Sighted = false
                        };
                        _context.Add(checklistItem);
                    }
                    await _context.SaveChangesAsync();
              
                }
                return RedirectToAction(nameof(Index));
                
            }
            var birds = _context.Birds.ToList();
            ViewBag.Birds = new SelectList(birds, "Id", "CommonName");
    
            return View(checklist);
        }




        // GET: Checklists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound("User not found");
            }
            
            if (id == null)
            {
                return NotFound();
            }

            var checklist = await _context.Checklists
                .Include(c => c.ChecklistItems)
                    .ThenInclude(ci => ci.Bird)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (checklist == null)
            {
                return NotFound();
            }

            var allBirds = await _context.Birds
            .Select(b => new {
                b.Id,
                CommonName = b.CommonName ?? "(Unnamed bird)"
            })
            .ToListAsync();

            var selectedBirdIds = checklist.ChecklistItems.Select(c => c.BirdId).ToList();

            ViewBag.AllBirds = new MultiSelectList(allBirds, "Id", "CommonName", selectedBirdIds);

            ViewBag.SelectedUserId = checklist.SdUserId;

            return View(checklist);
        }

        // POST: Checklists/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ChecklistName,SdUserId")] Checklist checklist, int[] selectedBirds)
        {
            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound("User not found");
            }

            if (id != checklist.Id)
            {
                return NotFound();
            }

            if (selectedBirds == null || selectedBirds.Length == 0)
            {
                ModelState.AddModelError("", "You must select at least one bird");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingChecklist = await _context.Checklists
                        .Include(c => c.ChecklistItems)
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (existingChecklist == null)
                    {
                        return NotFound();
                    }

                    existingChecklist.ChecklistName = checklist.ChecklistName;
                    

                    // To update birds
                    var existingBirdIds = existingChecklist.ChecklistItems.Select(ci => ci.BirdId).ToList();
                    var selectedBirdIds = selectedBirds.Select(sb => (int?)sb).ToList();

                    var birdsToRemove = existingBirdIds.Except(selectedBirdIds).ToList();
                    foreach (var birdId in birdsToRemove)
                    {
                        var itemToRemove = existingChecklist.ChecklistItems
                            .FirstOrDefault(ci => ci.BirdId == birdId);
                        if (itemToRemove != null)
                        {
                            _context.ChecklistItems.Remove(itemToRemove);
                        }
                    }
                    

                    var birdsToAdd = selectedBirdIds.Except(existingBirdIds).ToList();
                    foreach (var birdId in birdsToAdd)
                    {

                        existingChecklist.ChecklistItems.Add(new ChecklistItem
                        {
                            BirdId = birdId,
                            Sighted = false
                        });
                    }
                    
                    
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));

                }


                catch (DbUpdateConcurrencyException)
                {
                    if (!ChecklistExists(checklist.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            var allBirds = await _context.Birds.ToListAsync();

            ViewBag.AllBirds = new MultiSelectList(allBirds, "Id", "CommonName", selectedBirds);
            return View(checklist);
        }



        // GET: Checklists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Get the current user's SdUser
        var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
        if (sdUser == null)
        {
            return NotFound("User not found");
        }
            if (id == null)
            {
                return NotFound();
            }

            var checklist = await _context.Checklists
                .Include(c => c.SdUser)
                .Include(c => c.ChecklistItems)
                    .ThenInclude(ci => ci.Bird)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (checklist == null)
            {
                return NotFound();
            }

            return View(checklist);
        }



        // POST: Checklists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var checklist = await _context.Checklists
                .Include(c => c.ChecklistItems)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (checklist == null)
            {
                return NotFound();
            }

            try
            {
                // Remove all related ChecklistItems first
                _context.ChecklistItems.RemoveRange(checklist.ChecklistItems);

                // Remove the checklist itself
                _context.Checklists.Remove(checklist);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Redirect to the Index action with the userId parameter
                return RedirectToAction(nameof(Index), new { userId = checklist.SdUserId });

            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors (e.g., foreign key constraints)
                ModelState.AddModelError("", "Unable to delete the checklist. It may have related data that cannot be deleted.");
                return View("Delete", checklist); // Return to the Delete view with an error message
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                ModelState.AddModelError("", "An unexpected error occurred while deleting the checklist.");
                return View("Delete", checklist); // Return to the Delete view with an error message
            }

        }

        private bool ChecklistExists(int id)
        {
            return _context.Checklists.Any(e => e.Id == id);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSighted(int id, bool sighted)
        {
            try
            {
                var checklistItem = await _context.ChecklistItems
                    .Include(ci => ci.Checklist)
                        .ThenInclude(c => c.ChecklistItems)
                    .FirstOrDefaultAsync(ci => ci.Id == id);

                if (checklistItem == null) return Json(new { success = false });

                checklistItem.Sighted = sighted;
                //_context.Update(checklistItem);
                await _context.SaveChangesAsync();

                // To calculate the spotted and total birds
                var total = checklistItem.Checklist.ChecklistItems.Count;
                var spotted = checklistItem.Checklist.ChecklistItems.Count(ci => ci.Sighted ?? false );

                

                return Json(new { 
                    success = true,
                    total = total,
                    spotted = spotted,
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkComplete([FromBody]int id)
        {
            try
            {
                var checklist = await _context.Checklists
                    .Include(c => c.ChecklistItems)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (checklist == null) return Json(new { success = false });

                
                foreach (var item in checklist.ChecklistItems)
                {
                    item.Sighted = true; // Mark all items as sighted
                }
                


                //_context.Update(checklist);
                await _context.SaveChangesAsync();

                return Json(new 
                { 
                    success = true,
                    total = checklist.ChecklistItems,
                    spotted = checklist.ChecklistItems.Count(ci => ci.Sighted ?? true)
                    });
            }
                    
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
           
            }
        }

        // In ChecklistsController.cs

        [HttpGet]
        public async Task<IActionResult> GetUserChecklists()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
            if (sdUser == null)
            {
                return NotFound();
            }

            var checklists = await _context.Checklists
                .Where(c => c.SdUserId == sdUser.Id)
                .Select(c => new { id = c.Id, name = c.ChecklistName })
                .ToListAsync();

            return Json(checklists);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBirdToChecklist(int birdId, int? checklistId, string checklistName)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return Unauthorized();
                }

                var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
                if (sdUser == null)
                {
                    return NotFound("User not found");
                }

                // Validate either checklistId or checklistName must be provided
                if (!checklistId.HasValue && string.IsNullOrWhiteSpace(checklistName))
                {
                    return BadRequest("Either select an existing checklist or provide a name for a new one");
                }

                Checklist checklist;
                
                if (checklistId.HasValue)
                {
                    // Add to existing checklist
                    checklist = await _context.Checklists
                        .Include(c => c.ChecklistItems)
                        .FirstOrDefaultAsync(c => c.Id == checklistId && c.SdUserId == sdUser.Id);
                    
                    if (checklist == null)
                    {
                        return NotFound("Checklist not found");
                    }
                }
                else
                {
                    // Create new checklist
                    checklist = new Checklist
                    {
                        ChecklistName = checklistName.Trim(),
                        SdUserId = sdUser.Id
                    };
                    
                    _context.Add(checklist);
                    await _context.SaveChangesAsync();
                }

                // Check if bird already exists in checklist
                if (!checklist.ChecklistItems.Any(ci => ci.BirdId == birdId))
                {
                    var checklistItem = new ChecklistItem
                    {
                        ChecklistId = checklist.Id,
                        BirdId = birdId,
                        Sighted = false
                    };
                    
                    _context.Add(checklistItem);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Bird added to checklist successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


 
        

        
    }

     
}