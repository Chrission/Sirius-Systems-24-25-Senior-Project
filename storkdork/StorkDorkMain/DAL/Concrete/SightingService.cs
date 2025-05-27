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
    private readonly StorkDorkDbContext _context;
    private readonly UserManager<IdentityUser> _usermanager;
    private readonly IEBirdService _eBirdService;
    private const double DEFAULT_LAT = 44.8485; // Monmouth, OR
    private const double DEFAULT_LNG = -123.2340;

    public SightingService(StorkDorkDbContext context, UserManager<IdentityUser> userManager, IEBirdService eBirdService)
    {
        _context = context;
        _usermanager = userManager;
        _eBirdService = eBirdService;
    }

    public async Task<List<SightMarker>> GetSightingsAsync()
    {
        return await _context.Sightings
            .Include(s => s.Bird)
            .Include(s => s.SdUser.UserSettings)
            .Select(s => new SightMarker
            {
                SightingId = s.Id,
                userId = s.SdUserId,
                CommonName = s.Bird.CommonName ?? "Unkown Name",
                SciName = s.Bird.ScientificName,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                Description = s.Notes,
                Date = s.Date,
                Country = s.Country,
                Subdivision = s. Subdivision,
                Birder = s.SdUser.UserSettings.AnonymousSightings ? "Anonymous" : $"{s.SdUser.FirstName}, {s.SdUser.LastName}",
                BirdId = s.Bird.Id,
            })
            .ToListAsync();
    }

    public async Task<List<SightMarker>> GetSightingsByUserIdAsync(int userId)
    {
        return await _context.Sightings
            .Where(s => s.SdUserId == userId)
            .Include(s => s.Bird)
            .Include(s => s.SdUser.UserSettings)
            .Select(s => new SightMarker
            {
                SightingId = s.Id,
                userId = s.SdUserId,
                CommonName = s.Bird.CommonName ?? "Unkown Name",
                SciName = s.Bird.ScientificName,
                Longitude = s.Longitude,
                Latitude = s.Latitude,
                Description = s.Notes,
                Date = s.Date,
                Country = s.Country,
                Subdivision = s.Subdivision,
                Birder = s.SdUser.UserSettings.AnonymousSightings ? "Anonymous" : $"{s.SdUser.FirstName}, {s.SdUser.LastName}",
                BirdId = s.Bird.Id,
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
    
    public async Task UpdateSightingLocationAsync(int sightingId, string country, string subdivision)
    {
        var sighting = await _context.Sightings.FindAsync(sightingId);
        if (sighting == null)
        {
            throw new KeyNotFoundException("Sighting not found.");
        }

        sighting.Country = country;
        sighting.Subdivision = subdivision;
        await _context.SaveChangesAsync();
    }

    // public async Task<List<SightMarker>> GetSightingsByCurrentUserAsync(int userId)
    // {
    //     string userID = _usermanager.GetUserId(User);
    //     return await 
    // }
}