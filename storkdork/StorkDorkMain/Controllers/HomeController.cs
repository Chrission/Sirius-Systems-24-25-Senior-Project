using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.Models;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Data;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;

namespace StorkDorkMain.Controllers;

public class HomeController : Controller
{
    private readonly StorkDorkDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, StorkDorkDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        // Test Database Connection
        // try
        // {
        //     var testConnection = _context.Birds.FirstOrDefault();
        //     if (testConnection != null)
        //     {
        //         _logger.LogInformation("Database Connected!");
        //     }
        //     else
        //     {
        //         _logger.LogWarning("Database connection successful, but no bird data founs.");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     _logger.LogError($"Database connection failed: {ex.Message}");
        // }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }


    public IActionResult Map()
    {
        return View();
    }




    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View(new ErrorViewModel { RequestId = requestId });
    }

}
