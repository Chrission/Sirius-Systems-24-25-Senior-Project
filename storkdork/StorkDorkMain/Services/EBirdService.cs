using System.Text.Json;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Models;

namespace StorkDorkMain.Services;
public interface IEBirdService
{
    Task<IEnumerable<Sighting>> GetNearestSightings(int birdId, double lat, double lng, int maxResults = 10);
    Task<IEnumerable<NearbySighting>> GetNearbySightings(double lat, double lng, int radius = 25);
}

public class EBirdService : IEBirdService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IBirdRepository _birdRepository;

    public EBirdService(HttpClient httpClient, IConfiguration configuration, IBirdRepository birdRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _birdRepository = birdRepository;
        _httpClient.DefaultRequestHeaders.Add("X-eBirdApiToken", _configuration["EBird:ApiKey"]);
    }

    public async Task<IEnumerable<Sighting>> GetNearestSightings(int birdId, double lat, double lng, int maxResults = 10)
    {
        try
        {
            // Get bird details using repository
            var bird = _birdRepository.FindById(birdId);
            if (bird == null || string.IsNullOrEmpty(bird.SpeciesCode))
            {
                Console.WriteLine($"Bird not found or invalid species code for ID: {birdId}");
                return new List<Sighting>();
            }

            var url = $"https://api.ebird.org/v2/data/nearest/geo/recent/{bird.SpeciesCode}?lat={lat}&lng={lng}&maxResults={maxResults}";
            var response = await _httpClient.GetAsync(url);
            
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"eBird API Response for {bird.CommonName}: {content}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"eBird API error: {response.StatusCode}");
                return new List<Sighting>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var eBirdSightings = await JsonSerializer.DeserializeAsync<List<EBirdSighting>>(
                await response.Content.ReadAsStreamAsync(),
                options);

            if (eBirdSightings == null)
            {
                return new List<Sighting>();
            }

            // Map EBirdSighting to your Sighting model with BirdId
            return eBirdSightings.Select(es => new Sighting
            {
                BirdId = birdId,
                Latitude = (decimal?)es.Latitude,
                Longitude = (decimal?)es.Longitude,
                Date = DateTime.Parse(es.ObservationDate),
                Notes = $"Count: {es.Count}, Location: {es.LocationName}"
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNearestSightings: {ex}");
            return new List<Sighting>();
        }
    }

    public async Task<IEnumerable<NearbySighting>> GetNearbySightings(double lat, double lng, int radius = 25)
    {
        try
        {
            var apiKey = _configuration["EBird:ApiKey"];
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-eBirdApiToken", apiKey);
            
            // Use the eBird recent nearby observations endpoint
            var url = $"https://api.ebird.org/v2/data/obs/geo/recent?lat={lat}&lng={lng}&dist={radius}&back=30";
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"eBird API error: {response.StatusCode}");
                return new List<NearbySighting>();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var eBirdSightings = await JsonSerializer.DeserializeAsync<List<EBirdSighting>>(
                await response.Content.ReadAsStreamAsync(),
                options);

            if (eBirdSightings == null)
            {
                return new List<NearbySighting>();
            }

            // Group by species to keep only the most recent sighting for each
            return eBirdSightings
                .GroupBy(s => s.SpeciesCode)
                .Select(g => g.OrderByDescending(s => DateTime.Parse(s.ObservationDate)).First())
                .Select(es => {
                    // Try to find matching bird by species code
                    var bird = _birdRepository.GetBirdBySpeciesCode(es.SpeciesCode);
                    
                    return new NearbySighting
                    {
                        SpeciesCode = es.SpeciesCode,
                        BirdId = bird?.Id, // Add the BirdId if found
                        CommonName = es.CommonName,
                        ScientificName = es.ScientificName,
                        LocationName = es.LocationName,
                        ObservationDate = es.ObservationDate,
                        Count = es.Count,
                        Latitude = es.Latitude,
                        Longitude = es.Longitude
                    };
                })
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNearbySightings: {ex}");
            return new List<NearbySighting>();
        }
    }
}