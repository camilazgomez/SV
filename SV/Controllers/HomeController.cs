using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SV.Models;
using System.Diagnostics;

namespace SV.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly InscripcionesBrDbContext _context;

        public HomeController(ILogger<HomeController> logger, InscripcionesBrDbContext contex)
        {
            _logger = logger;
            _context = contex;  
        }

        public IActionResult Index()
        {
            return View();
        }

    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}