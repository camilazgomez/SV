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

        public async Task<IActionResult> Privacy()
           
        {
      
            if (_context.MultiOwners == null)
            {
                return RedirectToAction("index", "RealStateForm");
            }

            return _context.MultiOwners != null ?

                          View(await _context.MultiOwners.ToListAsync()) :
                          Problem("Entity set 'InscripcionesBrDbContext.RealStateForms'  is null.");
           
        }

        [HttpPost]
        public IActionResult MultiOwnerQuery()
        {
            string commune = Request.Form["commune"];
            string block = Request.Form["block"];
            string property = Request.Form["property"];
            int year = int.Parse(Request.Form["year"]);
            System.Diagnostics.Debug.WriteLine(year);
            return View();
        }





        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}