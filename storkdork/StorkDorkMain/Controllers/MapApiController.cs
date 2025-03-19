using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Models;
using StorkDorkMain.Models.DTO;
using StorkDorkMain.Services;
using System.Linq;

namespace StorkDorkMain.Controllers;

// Handles requests related to the map page
[Route("api/map")]
[ApiController]
public class MapApiController : ControllerBase
{
    private readonly ISightingService _sightingService;
    private readonly ISDUserRepository _sdUserRepository;
<<<<<<< HEAD
    // private readonly UserManager<IdentityUser> _userManager;
    private readonly IEBirdService _eBirdService;

    public MapApiController(ISightingService sightingService, ISDUserRepository sdUserRepository, /*UserManager<IdentityUser> userManager,*/ IEBirdService eBirdService)
    {
        _sightingService = sightingService;
        _sdUserRepository = sdUserRepository;
        // _userManager = userManager;
=======
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IEBirdService _eBirdService;

    public MapApiController(ISightingService sightingService, ISDUserRepository sdUserRepository, UserManager<IdentityUser> userManager, IEBirdService eBirdService)
    {
        _sightingService = sightingService;
        _sdUserRepository = sdUserRepository;
        _userManager = userManager;
>>>>>>> dev
        _eBirdService = eBirdService;
    }

    // Grabs all sightings in the database
    // Expectations: returns a list of sightings to javascript
    [HttpGet]
    [Route("GetSightings")]
    public async Task<IActionResult> GetSightings()
    {
        try
        {
            var sightings = await _sightingService.GetSightingsAsync();
            return Ok(sightings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

<<<<<<< HEAD
=======
    // Grabs all sightings belonging to a userId
    // Expectations: returns sightings for specified user
    // [HttpGet]
    // [Route("GetSightings/{userId}/1")]
    // public async Task<IActionResult> GetSightingsByUserId(int userId)
    // {
    //     if (userId <= 0)
    //     {
    //         return BadRequest("Invalid user ID.");
    //     }
        
    //     try
    //     {
    //         var sightings = await _sightingService.GetSightingsByUserIdAsync(userId);
    //         return Ok(sightings);
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Internal Server Error: {ex.Message}");
    //     }
    // }

>>>>>>> dev
    // Grabs all sightings belonging to the currently logged in user
    [HttpGet]
    [Route("GetSightings/{userId}")]
    public async Task<IActionResult> GetSightingsByUser(int userId)
    {
        if (userId <= 0)
        {
            return BadRequest("Invalid user ID.");
        }
        
        try
        {
            var sightings = await _sightingService.GetSightingsByUserIdAsync(userId);
            return Ok(sightings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }

    [HttpGet]
    [Route("GetNearestSightings")]
    public async Task<IActionResult> GetNearestSightings(
        int birdId, 
        double? lat = null, 
        double? lng = null)
    {
        try
        {
            var latitude = lat ?? 44.8485; // Default to Monmouth, OR
            var longitude = lng ?? -123.2340;

            var sightings = await _eBirdService.GetNearestSightings(birdId, latitude, longitude);
            
            // Ensure coordinates are valid before returning
            var validSightings = sightings.Where(s => 
                s?.Latitude != null && s?.Longitude != null && 
                Math.Abs(s.Latitude.Value) <= 90 && Math.Abs(s.Longitude.Value) <= 180
            );

            return Ok(validSightings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Unable to fetch sightings" });
        }
    }
}