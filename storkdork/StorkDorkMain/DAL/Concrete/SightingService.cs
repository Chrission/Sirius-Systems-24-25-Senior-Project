using Microsoft.EntityFrameworkCore;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Data;
using StorkDorkMain.Models.DTO;
using StorkDork.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using StorkDorkMain.Models;
using StorkDorkMain.Services;

namespace StorkDorkMain.DAL.Concrete;

public class SightingService : ISightingService
{
    private readonly StorkDorkContext _storkDorkContext;
    private readonly UserManager<IdentityUser> _usermanager;
    private readonly IEBirdService _eBirdService;
    private const double DEFAULT_LAT = 44.8485; // Monmouth, OR
    private const double DEFAULT_LNG = -123.2340;

    public SightingService(StorkDorkContext storkDorkContext, UserManager<IdentityUser> userManager, IEBirdService eBirdService)
    {
        _storkDorkContext = storkDorkContext;
        _usermanager = userManager;
        _eBirdService = eBirdService;
    }

    public async Task<List<SightMarker>> GetSightingsAsync()
    {
        return await _storkDorkContext.Sightings
            .Include(s => s.Bird)
            .Select(s => new SightMarker
            {
                CommonName = s.Bird.CommonName ?? "Unknown Name",
                SciName = s.Bird.ScientificName,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                Description = s.Notes,
                Date = s.Date
            })
            .ToListAsync();
    }

    public async Task<List<SightMarker>> GetSightingsByUserIdAsync(int userId)
    {
        return await _storkDorkContext.Sightings
            .Where(s => s.SdUserId == userId)
            .Include(s => s.Bird)
            .Select(s => new SightMarker
            {
                CommonName = s.Bird.CommonName ?? "Unknown Name",
                SciName = s.Bird.ScientificName,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                Description = s.Notes,
                Date = s.Date
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<Sighting>> GetNearestSightingsForBird(int birdId, double? lat = null, double? lng = null)
    {
        var latitude = lat ?? DEFAULT_LAT;
        var longitude = lng ?? DEFAULT_LNG;

        var eBirdSightings = await _eBirdService.GetNearestSightings(birdId, latitude, longitude);

        return eBirdSightings;
    }

    // public async Task<List<SightMarker>> GetSightingsByCurrentUserAsync(int userId)
    // {
    //     string userID = _usermanager.GetUserId(User);
    //     return await 
    // }
}