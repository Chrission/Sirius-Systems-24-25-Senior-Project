using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.Models;
using StorkDorkMain.Services;
using StorkDorkMain.DAL.Abstract;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace StorkDorkMain.Controllers
{
    public class NearbySightingsController : Controller
    {
        private readonly IEBirdService _eBirdService;
        private readonly IBirdRepository _birdRepository;

        public NearbySightingsController(IEBirdService eBirdService, IBirdRepository birdRepository)
        {
            _eBirdService = eBirdService;
            _birdRepository = birdRepository;
        }

        public IActionResult Index(decimal lat, decimal lng)
        {
            // if lat and lng are provided, use them as defaults
            if (lat != 0 && lng != 0)
            {
                var viewModel = new NearbySightingsViewModel
                {
                    Radius = 25, // Default radius in km
                    DefaultLatitude = lat,
                    DefaultLongitude = lng
                };

                return View(viewModel);
            }
            else
            {
                return DefaultView();
            };
        }
        private IActionResult DefaultView()
        {
            // Default values
            var viewModel = new NearbySightingsViewModel
            {
                Radius = 25, // Default radius in km
                DefaultLatitude = 44.8485m, // Monmouth, OR as default
                DefaultLongitude = -123.2340m
            };
            
            return View(viewModel);
        }


        [HttpGet]
        public IActionResult FromLocation(decimal lat, decimal lng)
        {
            // Set up view model with given coordinates
            var viewModel = new NearbySightingsViewModel
            {
                Radius = 25, // Default radius in km
                DefaultLatitude = lat,
                DefaultLongitude = lng
            };
            
            return View("Index", viewModel);
        }

        [HttpGet]
        [Route("api/nearbysightings")]
        public async Task<IActionResult> GetNearbySightings(double lat, double lng, int radius = 25)
        {
            if (radius < 1) radius = 1;
            if (radius > 50) radius = 50; // Limit to 50km max as per eBird API limits
            
            try
            {
                var sightings = await _eBirdService.GetNearbySightings(lat, lng, radius);
                return Json(sightings);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}