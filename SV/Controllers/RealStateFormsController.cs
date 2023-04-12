using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using SV.Models;
using static System.Net.Mime.MediaTypeNames;


namespace SV.Controllers
{
    public class RealStateFormsController : Controller
    {
        private readonly InscripcionesBrDbContext _context;

        public RealStateFormsController(InscripcionesBrDbContext context)
        {
            _context = context;
        }

        // GET: RealStateForms
        public async Task<IActionResult> Index()
        {
              return _context.RealStateForms != null ? 
                          View(await _context.RealStateForms.ToListAsync()) :
                          Problem("Entity set 'InscripcionesBrDbContext.RealStateForms'  is null.");
        }

        // GET: RealStateForms/Details/5
        public async Task<IActionResult> Details(int? id)
        {

         
            if (id == null || _context.RealStateForms == null)
            {
                return NotFound();
            }

            var realStateForm = await _context.RealStateForms
                .FirstOrDefaultAsync(m => m.AttentionNumber == id);
            if (realStateForm == null)
            {
                return NotFound();
            }
            expandedDetailsOfForms viewData = new();
            viewData.Sellers = _context.People.Where(s=> s.FormsId == id && s.Seller == true).ToList();
            viewData.Buyers = _context.People.Where(s => s.FormsId == id && s.Seller == false).ToList();
            viewData.realStateForm = realStateForm;
            return View(viewData);
        }

        // GET: RealStateForms/Create
        public IActionResult Create()
        {
            ViewBag.Communes = _context.Commune.ToList();
            
            return View();
        }


  

        // POST: RealStateForms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AttentionNumber,NatureOfTheDeed,Commune,Block,Property,Sheets,InscriptionDate,InscriptionNumber")] RealStateForm realStateForm)
        {
            System.Diagnostics.Debug.WriteLine("WIIIIII");
            System.Diagnostics.Debug.WriteLine(Request.Form["Commune"]);

            if (ModelState.IsValid)
            {
                _context.Add(realStateForm);
                await _context.SaveChangesAsync();

              
                for (int seller = 1; seller < Request.Form["rutSeller"].Count; seller++)
                {
                    try
                    {
                        double? ownershipPercentage;
                        checkRut(Request.Form["rutSeller"][seller]);
                        if (Request.Form["ownershipPercentageSeller"][seller] == null)
                        {
                            ownershipPercentage = null;
                        }
                        else
                        {
                            ownershipPercentage = double.Parse(Request.Form["ownershipPercentageSeller"][seller], CultureInfo.InvariantCulture);
                            checkOwnershipPercentage(ownershipPercentage);
                        }

                        Person newSeller = new();
                        newSeller.Rut = Request.Form["rutSeller"][seller];
                        newSeller.OwnershipPercentage= ownershipPercentage;
                        newSeller.UncreditedOwnership = bool.Parse(Request.Form["uncreditedClickedSeller"][seller]);
                        newSeller.Seller = true;
                        newSeller.Heir = false;
                        newSeller.FormsId = _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault().AttentionNumber;
                        _context.Add(newSeller);
                        await _context.SaveChangesAsync();


                    }
                    // Most specific:
                    catch (ArgumentNullException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} First exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }
                    // Least specific:
                    catch (FormatException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} Second exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }
                    catch (ArithmeticException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} Third exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }

                }

                for (int buyer = 1; buyer < Request.Form["rutBuyer"].Count; buyer++)
                {
                    try
                    {
                        double? ownershipPercentage;
                        checkRut(Request.Form["rutBuyer"][buyer]);
                        if (Request.Form["ownershipPercentageSeller"][buyer] == null)
                        {
                            ownershipPercentage = null;
                        }
                        else
                        {
                            ownershipPercentage = double.Parse(Request.Form["ownershipPercentageBuyer"][buyer], CultureInfo.InvariantCulture);
                            checkOwnershipPercentage(ownershipPercentage);
                        }

                        Person newBuyer = new();
                        newBuyer.Rut = Request.Form["rutBuyer"][buyer];
                        newBuyer.OwnershipPercentage = ownershipPercentage;
                        newBuyer.UncreditedOwnership = bool.Parse(Request.Form["uncreditedClickedBuyer"][buyer]);
                        newBuyer.Seller = false;
                        newBuyer.Heir = true;
                        newBuyer.FormsId = _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault().AttentionNumber;
                        _context.Add(newBuyer);
                        await _context.SaveChangesAsync();
                    }
                    catch (ArgumentNullException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} First exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }
                    // Least specific:
                    catch (FormatException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} Second exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }
                    catch (ArithmeticException e)
                    {
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                        System.Diagnostics.Debug.WriteLine("{0} Third exception caught.", e);
                        System.Diagnostics.Debug.WriteLine("********************************************************************");
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            ViewBag.Communes = _context.Commune.ToList();
            return View(realStateForm);
        }
        
        // GET: RealStateForms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RealStateForms == null)
            {
                return NotFound();
            }

            var realStateForm = await _context.RealStateForms.FindAsync(id);
            if (realStateForm == null)
            {
                return NotFound();
            }
            return View(realStateForm);
        }

        // POST: RealStateForms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AttentionNumber,NatureOfTheDeed,Commune,Block,Property,Sheets,InscriptionDate,InscriptionNumber")] RealStateForm realStateForm)
        {
            if (id != realStateForm.AttentionNumber)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(realStateForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RealStateFormExists(realStateForm.AttentionNumber))
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
            return View(realStateForm);
        }

        // GET: RealStateForms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RealStateForms == null)
            {
                return NotFound();
            }

            var realStateForm = await _context.RealStateForms
                .FirstOrDefaultAsync(m => m.AttentionNumber == id);
            if (realStateForm == null)
            {
                return NotFound();
            }

            return View(realStateForm);
        }

        // POST: RealStateForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RealStateForms == null)
            {
                return Problem("Entity set 'InscripcionesBrDbContext.RealStateForms'  is null.");
            }
            var realStateForm = await _context.RealStateForms.FindAsync(id);
            if (realStateForm != null)
            {
                _context.RealStateForms.Remove(realStateForm);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RealStateFormExists(int id)
        {
          return (_context.RealStateForms?.Any(e => e.AttentionNumber == id)).GetValueOrDefault();
        }

        static void checkRut(string rut)
        {
            if (rut == "")
            {
                throw new ArgumentNullException(paramName: nameof(rut), message: "Parameter can't be null");
            }
        }

        static void checkOwnershipPercentage(double? ownershipPercentage)
        {
            if (ownershipPercentage < 0)
            {
                throw new ArithmeticException("Percentage must be greater than 0");
            }
            else if (ownershipPercentage > 100)
            {
                throw new ArithmeticException("Percentage must be lower than 100");
            }
        }
    }
}

