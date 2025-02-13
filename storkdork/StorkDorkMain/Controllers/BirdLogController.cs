using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Data;
using StorkDorkMain.Models;

namespace StorkDork.Controllers
{
    public class BirdLogController : Controller
    {
        private readonly StorkDorkContext _context;

        public BirdLogController(StorkDorkContext context)
        {
            _context = context;
        }

        // GET: BirdLog
        public async Task<IActionResult> Index()
        {
            
            var sightings = await _context.Sightings
                .Include(s => s.Bird)  // Ensure Bird data is loaded
                .Include(s => s.Sduser) //Include Sduser data for each sighting
                .ToListAsync();

            // A predefined list of common PNW bird sighting locations with longitude and latitde coordinates
            ViewBag.PnwLocations = new Dictionary<string, string>
            {
                { "48.4244,-122.3358", "Skagit Valley, WA" },
                { "46.8797,-121.7269", "Mount Rainier National Park, WA" },
                { "47.6573,-122.4057", "Discovery Park, Seattle, WA" },
                { "47.0726,-122.7175", "Nisqually National Wildlife Refuge, WA" },
                { "47.8601,-123.9343", "Olympic National Park (Hoh Rainforest), WA" },
                { "45.7156,-122.7745", "Sauvie Island, OR" },
                { "42.9778,-118.9097", "Malheur National Wildlife Refuge, OR" },
                { "42.8684,-122.1685", "Crater Lake National Park, OR" },
                { "45.9190,-123.9740", "Ecola State Park, OR" },
                { "42.1561,-121.7381", "Klamath Basin, OR" },
                { "49.0456,-123.0586", "Boundary Bay, BC" },
                { "49.3043,-123.1443", "Stanley Park, Vancouver, BC" },
                { "49.1167,-123.1500", "Reifel Migratory Bird Sanctuary, BC" },
                { "48.7500,-125.5000", "Pacific Rim National Park, BC" },
                { "49.5000,-119.5833", "Okanagan Valley, BC" },
                { "47.5000,-116.8000", "Lake Coeur d’Alene, ID" },
                { "43.3000,-112.0000", "Camas National Wildlife Refuge, ID" },
                { "44.3611,-111.4550", "Harriman State Park, ID" }
            };

            
            return View(sightings);
           
        }

        // GET: BirdLog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sighting = await _context.Sightings
                .Include(s => s.Bird)
                .Include(s => s.Sduser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sighting == null)
            {
                return NotFound();
            }

              // A predefined list of common PNW bird sighting locations with longitude and latitde coordinates
            ViewBag.PnwLocations = new Dictionary<string, string>
            {
                { "48.4244,-122.3358", "Skagit Valley, WA" },
                { "46.8797,-121.7269", "Mount Rainier National Park, WA" },
                { "47.6573,-122.4057", "Discovery Park, Seattle, WA" },
                { "47.0726,-122.7175", "Nisqually National Wildlife Refuge, WA" },
                { "47.8601,-123.9343", "Olympic National Park (Hoh Rainforest), WA" },
                { "45.7156,-122.7745", "Sauvie Island, OR" },
                { "42.9778,-118.9097", "Malheur National Wildlife Refuge, OR" },
                { "42.8684,-122.1685", "Crater Lake National Park, OR" },
                { "45.9190,-123.9740", "Ecola State Park, OR" },
                { "42.1561,-121.7381", "Klamath Basin, OR" },
                { "49.0456,-123.0586", "Boundary Bay, BC" },
                { "49.3043,-123.1443", "Stanley Park, Vancouver, BC" },
                { "49.1167,-123.1500", "Reifel Migratory Bird Sanctuary, BC" },
                { "48.7500,-125.5000", "Pacific Rim National Park, BC" },
                { "49.5000,-119.5833", "Okanagan Valley, BC" },
                { "47.5000,-116.8000", "Lake Coeur d’Alene, ID" },
                { "43.3000,-112.0000", "Camas National Wildlife Refuge, ID" },
                { "44.3611,-111.4550", "Harriman State Park, ID" }
            };

            return View(sighting);
        }

        // GET: BirdLog/Create
        public IActionResult Create()
        {
            ViewData["BirdId"] = new SelectList(_context.Birds, "Id", "CommonName");
            ViewData["SduserId"] = new SelectList(_context.Sdusers, "Id", "Id");

            
            ViewBag.PnwLocations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Skagit Valley, WA", Value = "48.4244,-122.3358" },
                new SelectListItem { Text = "Mount Rainier National Park, WA", Value = "46.8797,-121.7269" },
                new SelectListItem { Text = "Discovery Park, Seattle, WA", Value = "47.6573,-122.4057" },
                new SelectListItem { Text = "Nisqually National Wildlife Refuge, WA", Value = "47.0726,-122.7175" },
                new SelectListItem { Text = "Olympic National Park (Hoh Rainforest), WA", Value = "47.8601,-123.9343" },
                new SelectListItem { Text = "Sauvie Island, OR", Value = "45.7156,-122.7745" },
                new SelectListItem { Text = "Malheur National Wildlife Refuge, OR", Value = "42.9778,-118.9097" },
                new SelectListItem { Text = "Crater Lake National Park, OR", Value = "42.8684,-122.1685" },
                new SelectListItem { Text = "Ecola State Park, OR", Value = "45.9190,-123.9740" },
                new SelectListItem { Text = "Klamath Basin, OR", Value = "42.1561,-121.7381" },
                new SelectListItem { Text = "Boundary Bay, BC", Value = "49.0456,-123.0586" },
                new SelectListItem { Text = "Stanley Park, Vancouver, BC", Value = "49.3043,-123.1443" },
                new SelectListItem { Text = "Reifel Migratory Bird Sanctuary, BC", Value = "49.1167,-123.1500" },
                new SelectListItem { Text = "Pacific Rim National Park, BC", Value = "48.7500,-125.5000" },
                new SelectListItem { Text = "Okanagan Valley, BC", Value = "49.5000,-119.5833" },
                new SelectListItem { Text = "Lake Coeur d’Alene, ID", Value = "47.5000,-116.8000" },
                new SelectListItem { Text = "Camas National Wildlife Refuge, ID", Value = "43.3000,-112.0000" },
                new SelectListItem { Text = "Harriman State Park, ID", Value = "44.3611,-111.4550" }

            };



            
            return View();
        }

        // POST: BirdLog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SduserId,BirdId,Date,Latitude,Longitude,Notes")] Sighting sighting)
        {
            var selectedLocation = Request.Form["PnwLocation"];

            if (sighting.BirdId == null || sighting.BirdId == 0)
            {
                if (sighting.BirdId == 0) // 0 means you have selected "N/A"
                {
                    sighting.BirdId = null;
                }
                else
                {
                    ModelState.AddModelError("BirdId", "Please select a bird or enter 'N/A' if unknown");
                }
            }




            if (selectedLocation == "0")
            // If Latitude and Longitude are 0, set them to null
            {
                sighting.Latitude = null;
                sighting.Longitude = null;
            }

            else if (string.IsNullOrEmpty(selectedLocation))
            {
                ModelState.AddModelError("PnwLocation", "Please select a location or select N/A");
            }

            if ((sighting.BirdId == null || sighting.BirdId == 0) && string.IsNullOrEmpty(selectedLocation))
            {
                ModelState.AddModelError("","Both cannot be null");
            }


            if (ModelState.IsValid)
            {

                _context.Add(sighting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["BirdId"] = new SelectList(_context.Birds, "Id", "CommonName", sighting.BirdId);
            ViewData["SduserId"] = new SelectList(_context.Sdusers, "Id", "Id", sighting.SduserId);

                  
            ViewBag.PnwLocations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Skagit Valley, WA", Value = "48.4244,-122.3358" },
                new SelectListItem { Text = "Mount Rainier National Park, WA", Value = "46.8797,-121.7269" },
                new SelectListItem { Text = "Discovery Park, Seattle, WA", Value = "47.6573,-122.4057" },
                new SelectListItem { Text = "Nisqually National Wildlife Refuge, WA", Value = "47.0726,-122.7175" },
                new SelectListItem { Text = "Olympic National Park (Hoh Rainforest), WA", Value = "47.8601,-123.9343" },
                new SelectListItem { Text = "Sauvie Island, OR", Value = "45.7156,-122.7745" },
                new SelectListItem { Text = "Malheur National Wildlife Refuge, OR", Value = "42.9778,-118.9097" },
                new SelectListItem { Text = "Crater Lake National Park, OR", Value = "42.8684,-122.1685" },
                new SelectListItem { Text = "Ecola State Park, OR", Value = "45.9190,-123.9740" },
                new SelectListItem { Text = "Klamath Basin, OR", Value = "42.1561,-121.7381" },
                new SelectListItem { Text = "Boundary Bay, BC", Value = "49.0456,-123.0586" },
                new SelectListItem { Text = "Stanley Park, Vancouver, BC", Value = "49.3043,-123.1443" },
                new SelectListItem { Text = "Reifel Migratory Bird Sanctuary, BC", Value = "49.1167,-123.1500" },
                new SelectListItem { Text = "Pacific Rim National Park, BC", Value = "48.7500,-125.5000" },
                new SelectListItem { Text = "Okanagan Valley, BC", Value = "49.5000,-119.5833" },
                new SelectListItem { Text = "Lake Coeur d’Alene, ID", Value = "47.5000,-116.8000" },
                new SelectListItem { Text = "Camas National Wildlife Refuge, ID", Value = "43.3000,-112.0000" },
                new SelectListItem { Text = "Harriman State Park, ID", Value = "44.3611,-111.4550" }

            };

            
            return View(sighting);
        }

        // GET: BirdLog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sighting = await _context.Sightings.FindAsync(id);
            if (sighting == null)
            {
                return NotFound();
            }
               
        
            ViewData["BirdId"] = new SelectList(_context.Birds, "Id", "CommonName", sighting.BirdId);
            ViewData["SduserId"] = new SelectList(_context.Sdusers, "Id", "Id", sighting.SduserId);

            ViewBag.PnwLocations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Skagit Valley, WA", Value = "48.4244,-122.3358" },
                new SelectListItem { Text = "Mount Rainier National Park, WA", Value = "46.8797,-121.7269" },
                new SelectListItem { Text = "Discovery Park, Seattle, WA", Value = "47.6573,-122.4057" },
                new SelectListItem { Text = "Nisqually National Wildlife Refuge, WA", Value = "47.0726,-122.7175" },
                new SelectListItem { Text = "Olympic National Park (Hoh Rainforest), WA", Value = "47.8601,-123.9343" },
                new SelectListItem { Text = "Sauvie Island, OR", Value = "45.7156,-122.7745" },
                new SelectListItem { Text = "Malheur National Wildlife Refuge, OR", Value = "42.9778,-118.9097" },
                new SelectListItem { Text = "Crater Lake National Park, OR", Value = "42.8684,-122.1685" },
                new SelectListItem { Text = "Ecola State Park, OR", Value = "45.9190,-123.9740" },
                new SelectListItem { Text = "Klamath Basin, OR", Value = "42.1561,-121.7381" },
                new SelectListItem { Text = "Boundary Bay, BC", Value = "49.0456,-123.0586" },
                new SelectListItem { Text = "Stanley Park, Vancouver, BC", Value = "49.3043,-123.1443" },
                new SelectListItem { Text = "Reifel Migratory Bird Sanctuary, BC", Value = "49.1167,-123.1500" },
                new SelectListItem { Text = "Pacific Rim National Park, BC", Value = "48.7500,-125.5000" },
                new SelectListItem { Text = "Okanagan Valley, BC", Value = "49.5000,-119.5833" },
                new SelectListItem { Text = "Lake Coeur d’Alene, ID", Value = "47.5000,-116.8000" },
                new SelectListItem { Text = "Camas National Wildlife Refuge, ID", Value = "43.3000,-112.0000" },
                new SelectListItem { Text = "Harriman State Park, ID", Value = "44.3611,-111.4550" }

            };

            ViewBag.SelectedLatLong = $"{sighting.Latitude},{sighting.Longitude}";
            Console.WriteLine($"Selected Location: {ViewBag.SelectedLatLong}");

            return View(sighting);
        }

        // POST: BirdLog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SduserId,BirdId,Date,Latitude,Longitude,Notes")] Sighting sighting)
        {
            if (id != sighting.Id)
            {
                return NotFound();
            }
            
            var selectedLocation = Request.Form["PnwLocation"];

            if (sighting.BirdId == null || sighting.BirdId == 0)
            {
                if (sighting.BirdId == 0) // this means that "N/A" was selected
                {
                    sighting.BirdId = null;
                }
                else
                {  
                    ModelState.AddModelError("BirdId", "Please select a bird or enter 'N/A' if unknown");

                }
            

            if (selectedLocation == "0") // N/A was selected
            {
                sighting.Latitude = null;
                sighting.Longitude = null;
            }
            else if (string.IsNullOrEmpty(selectedLocation))
            {
                ModelState.AddModelError("PnwLocation","Please select a location or select N/A");
            }

            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sighting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SightingExists(sighting.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BirdId"] = new SelectList(_context.Birds, "Id", "CommonName", sighting.BirdId);
            ViewData["SduserId"] = new SelectList(_context.Sdusers, "Id", "Id", sighting.SduserId);

                  
            ViewBag.PnwLocations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Skagit Valley, WA", Value = "48.4244,-122.3358" },
                new SelectListItem { Text = "Mount Rainier National Park, WA", Value = "46.8797,-121.7269" },
                new SelectListItem { Text = "Discovery Park, Seattle, WA", Value = "47.6573,-122.4057" },
                new SelectListItem { Text = "Nisqually National Wildlife Refuge, WA", Value = "47.0726,-122.7175" },
                new SelectListItem { Text = "Olympic National Park (Hoh Rainforest), WA", Value = "47.8601,-123.9343" },
                new SelectListItem { Text = "Sauvie Island, OR", Value = "45.7156,-122.7745" },
                new SelectListItem { Text = "Malheur National Wildlife Refuge, OR", Value = "42.9778,-118.9097" },
                new SelectListItem { Text = "Crater Lake National Park, OR", Value = "42.8684,-122.1685" },
                new SelectListItem { Text = "Ecola State Park, OR", Value = "45.9190,-123.9740" },
                new SelectListItem { Text = "Klamath Basin, OR", Value = "42.1561,-121.7381" },
                new SelectListItem { Text = "Boundary Bay, BC", Value = "49.0456,-123.0586" },
                new SelectListItem { Text = "Stanley Park, Vancouver, BC", Value = "49.3043,-123.1443" },
                new SelectListItem { Text = "Reifel Migratory Bird Sanctuary, BC", Value = "49.1167,-123.1500" },
                new SelectListItem { Text = "Pacific Rim National Park, BC", Value = "48.7500,-125.5000" },
                new SelectListItem { Text = "Okanagan Valley, BC", Value = "49.5000,-119.5833" },
                new SelectListItem { Text = "Lake Coeur d’Alene, ID", Value = "47.5000,-116.8000" },
                new SelectListItem { Text = "Camas National Wildlife Refuge, ID", Value = "43.3000,-112.0000" },
                new SelectListItem { Text = "Harriman State Park, ID", Value = "44.3611,-111.4550" }

            };

            return View(sighting);
        }

        // GET: BirdLog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sighting = await _context.Sightings
                .Include(s => s.Bird)
                .Include(s => s.Sduser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sighting == null)
            {
                return NotFound();
            }

            return View(sighting);
        }

        // POST: BirdLog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sighting = await _context.Sightings.FindAsync(id);
            if (sighting != null)
            {
                _context.Sightings.Remove(sighting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SightingExists(int id)
        {
            return _context.Sightings.Any(e => e.Id == id);
        }
    }
}
