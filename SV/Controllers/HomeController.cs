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

        public async Task<IActionResult> Privacy(string comuna, string manzana, string predio, string year )
           
        {
            System.Diagnostics.Debug.WriteLine("*****");
            System.Diagnostics.Debug.WriteLine(comuna);
            System.Diagnostics.Debug.WriteLine("*****");
            if (_context.MultiOwners == null)
            {
                return RedirectToAction("index", "RealStateForm");
            }

            if (String.IsNullOrEmpty(comuna) || String.IsNullOrEmpty(manzana) || String.IsNullOrEmpty(predio))
            {
                return View(); 
                
            }
            int yearFormatted = int.Parse(year);

            return _context.MultiOwners != null ?

                          
                          View( _context.MultiOwners.Where(s=> s.Commune == comuna && s.Block == manzana &&  s.Property == predio && s.validityYearBegin <= yearFormatted && (s.validityYearFinish == null || s.validityYearFinish >= yearFormatted))):
                          Problem("Entity set 'InscripcionesBrDbContext.RealStateForms'  is null.");
           
        }

      



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}