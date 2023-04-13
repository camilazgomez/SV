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

        public async Task<IActionResult> MultiOwnerQuery(string commune, string block, string property, string year )
           
        {
            System.Diagnostics.Debug.WriteLine("*****");
            System.Diagnostics.Debug.WriteLine(commune);
            System.Diagnostics.Debug.WriteLine("*****");
            if (_context.MultiOwners == null)
            {
                return RedirectToAction("index", "RealStateForm");
            }

            if (String.IsNullOrEmpty(commune) || String.IsNullOrEmpty(block) || String.IsNullOrEmpty(property))
            {
                return View(); 
                
            }
            int yearFormatted = int.Parse(year);

            return _context.MultiOwners != null ?

                          
                          View( _context.MultiOwners.Where(s=> s.Commune == commune && s.Block == block &&  s.Property == property && s.ValidityYearBegin <= yearFormatted && (s.ValidityYearFinish == null || s.ValidityYearFinish >= yearFormatted))):
                          Problem("Entity set 'InscripcionesBrDbContext.RealStateForms'  is null.");
           
        }

      



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}