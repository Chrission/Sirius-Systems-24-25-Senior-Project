using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.Models;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using StorkDorkMain.DAL.Abstract;

namespace StorkDorkMain.Controllers;

public class UserSettingsController : Controller
{
    private readonly ILogger<UserSettingsController> _logger;
    private readonly ISDUserRepository _sdUserRepository;
    private readonly IUserSettingsRepository _userSettingsRepository;
    public UserSettingsController(ILogger<UserSettingsController> logger, ISDUserRepository sdUserRepository, IUserSettingsRepository userSettingsRepository)
    {
        _logger = logger;
        _sdUserRepository = sdUserRepository;
        _userSettingsRepository = userSettingsRepository;
    }

    public async Task<IActionResult> Settings()
    {
        var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
        if (sdUser == null)
            return Unauthorized();

        var settings = await _userSettingsRepository.GetSettingsByUserIdAsync(sdUser.Id);
        if (settings == null)
        {
            settings = (new UserSettings
            {
                SdUserId = sdUser.Id,
                AnonymousSightings = false
            });

            await _userSettingsRepository.CreateAsync(settings);
        }

        return View(settings);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateSettings(UserSettings model)
    {
        var sdUser = await _sdUserRepository.GetSDUserByIdentity(User);
        if (sdUser == null)
            return Unauthorized();

        model.SdUserId = sdUser.Id;

        var updated = await _userSettingsRepository.UpdateAsync(model);
        if (updated == null)
            return NotFound("Settings not found.");

        return RedirectToAction("Settings");
    }

    // public IActionResult Settings()
    // {
    //     return View();
    // }
    
}