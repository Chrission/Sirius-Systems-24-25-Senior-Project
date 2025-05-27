using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StorkDorkMain.Data;
using StorkDorkMain.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using StorkDorkMain.DAL.Abstract;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StorkDorkMain.DAL.Concrete;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;


namespace StorkDorkMain.Controllers
{
    public class BirdingTipsController : Controller
    {
        private readonly StorkDorkDbContext _context;

        public BirdingTipsController(StorkDorkDbContext context)
        {
            _context = context;
        }

        [HttpGet]

        public async Task<IActionResult> Index()
        {
            return View();

        }

        // Start of the Novice Section
        [HttpGet]
        public IActionResult Table_of_Contents()
        {
            return View("Novice_Birding_Tips/Table_of_Contents");

        }

        // Lesson #1
        [HttpGet]
        public IActionResult Introduction_To_Identification()
        {
            return View("Novice_Birding_Tips/Introduction_To_Identification");

        }

        // Lesson #2
        [HttpGet]
        public IActionResult Introduction_To_Technology()
        {
            return View("Novice_Birding_Tips/Introduction_To_Technology");

        }

        // Lesson #3
        [HttpGet]
        public IActionResult Introduction_To_Gear()
        {
            return View("Novice_Birding_Tips/Introduction_To_Gear");

        }

        // Lesson #4
        [HttpGet]
        public IActionResult Introduction_To_Etiquette()
        {
            return View("Novice_Birding_Tips/Introduction_To_Etiquette");

        }

        [HttpGet]
        public IActionResult Introduction_To_Habitats()
        {
            return View("Novice_Birding_Tips/Introduction_To_Habitats");

        }




        // Start of the Intermediate Section
        [HttpGet]
        public IActionResult Table_of_Contents_Intermediate()
        {
            return View("Intermediate_Birding_Tips/Table_of_Contents_Intermediate");

        }

        [HttpGet]
        public IActionResult Intermediate_Identification()
        {
            return View("Intermediate_Birding_Tips/Intermediate_Identification");

        }

        [HttpGet]
        public IActionResult Incorporating_Technology()
        {
            return View("Intermediate_Birding_Tips/Incorporating_Technology");

        }

        [HttpGet]
        public IActionResult Getting_Geared_Up()
        {
            return View("Intermediate_Birding_Tips/Getting_Geared_Up");

        }

        [HttpGet]
        public IActionResult Intermediate_Etiquette()
        {
            return View("Intermediate_Birding_Tips/Intermediate_Etiquette");

        }
        
        [HttpGet]
        public IActionResult Familiarizing_The_Habitat()
        {    
            return View("Intermediate_Birding_Tips/Familiarizing_The_Habitat");

        }
        
        
        



    }




}