using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StorkDorkMain.Models;
using Microsoft.EntityFrameworkCore;
using StorkDorkMain.Data;

namespace StorkDork.Controllers;

public class HomeController : Controller
{
    private readonly StorkDorkContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, StorkDorkContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
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
