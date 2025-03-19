using StorkDorkMain.Models;
using StorkDorkMain.Models.DTO;

namespace StorkDorkMain.DAL.Abstract;

public interface ISightingService
{
    Task<List<SightMarker>> GetSightingsAsync();
    Task<List<SightMarker>> GetSightingsByUserIdAsync(int userId);
<<<<<<< HEAD
    Task<IEnumerable<Sighting>> GetNearestSightingsForBird(int birdId, double? lat = null, double? lng = null);
    Task UpdateSightingLocationAsync(int sightingId, string country, string subdivision);

=======
>>>>>>> dev
    // Task<List<SightMarker>> GetSightingsByCurrentUserIdAsync(int userId);
}