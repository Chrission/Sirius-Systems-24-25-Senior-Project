using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StorkDorkMain.Data;
using StorkDorkMain.Models;

namespace StorkDorkMain.Controllers
{
    public class ChecklistsController : Controller
    {
        private readonly StorkDorkContext _context;

        public ChecklistsController(StorkDorkContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Index(int? userId)
        {
            // getting users for the user dropdown
            var users = await _context.SdUsers.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FirstName");

            //Pass the selected userId back to the view
            ViewBag.SelectedUserId = userId;


            // FIlter the cheklists by user ID if a user is selected
            IQueryable<Checklist> checklistsQuery = _context.Checklists
                .Include(c => c.SdUser)
                .Include(c => c.ChecklistItems)
                    .ThenInclude(ci => ci.Bird);
                

            
            //to check if there are no checklists for the user
            if (userId.HasValue)
            {
                checklistsQuery = checklistsQuery.Where(c => c.SdUserId == userId.Value);
            }
            else
            {
                checklistsQuery = checklistsQuery.Where(c => false);
            }

            var checklists = await checklistsQuery.ToListAsync();
            if (userId.HasValue && !checklists.Any())
            {
                ViewBag.NoChecklistsMessage = "No checklists found. Create one?";
            }
     
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
        public IActionResult Create(int? userId)
        {
            //redirect if no user is selected
            if (userId == null)
            {
                return RedirectToAction("Index");
            }

            // Fetch the details for the user
            var selectedUser = _context.SdUsers.FirstOrDefault(u => u.Id == userId);
            if (selectedUser == null)
            {
                return RedirectToAction("Index");
            }

            ViewBag.SelectedUserName = selectedUser.FirstName;

            // Fetch all birds from the database to display in the form
            var birds = _context.Birds.ToList();
            ViewBag.Birds = new SelectList(birds, "Id", "CommonName"); // Pass birds to the view

            ViewBag.SelectedUserId = userId;
            return View();
        }



        // POST: Checklists/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ChecklistName,SdUserId")] Checklist checklist, int[] selectedBirds)
        {
            
            if (ModelState.IsValid)
            {
                var userExists = await _context.SdUsers.AnyAsync(u => u.Id == checklist.SdUserId);
                if (!userExists)
                {
                    ModelState.AddModelError(" ", "Invalid user ID.");
                    ViewBag.SelectedUserId = checklist.SdUserId;
                    return View(checklist); // Return the view with an error message
                }

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
                return RedirectToAction("Index", new { userId = checklist.SdUserId });
                
            }

            //if the model is invalid, return create view with the user
            var selectedUser = _context.SdUsers.FirstOrDefault(u => u.Id == checklist.SdUserId);

            if (selectedUser != null)
            {
                ViewBag.SelectedUserName = selectedUser.FirstName;
            }
            ViewBag.SelectedUserId = checklist.SdUserId;
            var birds = _context.Birds.ToList();
            ViewBag.Birds = new SelectList(birds, "Id", "CommonName");
    
            return View(checklist);
        }




        // GET: Checklists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
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

            var allBirds = await _context.Birds.ToListAsync();
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
                return RedirectToAction("Index", new { userId = existingChecklist.SdUserId });

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
            ViewBag.SelectedUserId = checklist.SdUserId;
            return View(checklist);
        }



        // GET: Checklists/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

        

        
    }

     
}