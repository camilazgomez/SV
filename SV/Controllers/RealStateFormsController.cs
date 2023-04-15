using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
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
            viewData.RealStateForm = realStateForm;
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

            if (ModelState.IsValid)
            {
                _context.Add(realStateForm);
                await _context.SaveChangesAsync();

                if (Request.Form["NatureOfTheDeed"] != "Regularización de Patrimonio")
                {
                    for (int seller = 1; seller < Request.Form["rutSeller"].Count; seller++)
                    {
                        double? ownershipPercentage;
                        if (bool.Parse(Request.Form["uncreditedClickedSeller"][seller]))
                        {
                            ownershipPercentage = null;
                        }
                        else
                        {
                            ownershipPercentage = double.Parse(Request.Form["ownershipPercentageSeller"][seller], CultureInfo.InvariantCulture);
                        }

                        if (IsValidRut(Request.Form["rutSeller"][seller]) && IsValidOwnershipPercentage(ownershipPercentage))
                        {
                            Person newSeller = new();
                            newSeller.Rut = Request.Form["rutSeller"][seller];
                            newSeller.OwnershipPercentage= ownershipPercentage;
                            newSeller.UncreditedOwnership = bool.Parse(Request.Form["uncreditedClickedSeller"][seller]);
                            newSeller.Seller = true;
                            newSeller.Heir = false;
                            newSeller.FormsId = getLastFormsRecord(_context).AttentionNumber;
                            _context.Add(newSeller);
                            await _context.SaveChangesAsync();
                        }
                        else 
                        {
                            List<Person> peopleToRemoveFromDb = _context.People.Where(people => people.FormsId == getLastFormsRecord(_context).AttentionNumber).ToList();
                            _context.RemoveRange(peopleToRemoveFromDb);
                            await _context.SaveChangesAsync();
                            _context.Remove(realStateForm);
                            await _context.SaveChangesAsync();
                            ViewBag.Communes = _context.Commune.ToList();
                            return View(realStateForm);
                        }


                    }
                }


                List<string?> buyersOwnershipPercentage = Request.Form["ownershipPercentageBuyer"].ToList();
                bool isValidBuyerOwnershipPercentageSum = IsValidBuyersOwnershipPercentageSum(buyersOwnershipPercentage);

                for (int buyer = 1; buyer < Request.Form["rutBuyer"].Count; buyer++)
                {
                    double? ownershipPercentage;
                    if (bool.Parse(Request.Form["uncreditedClickedBuyer"][buyer]))
                    {
                        ownershipPercentage = null;
                    }
                    else
                    {
                        ownershipPercentage = double.Parse(Request.Form["ownershipPercentageBuyer"][buyer], CultureInfo.InvariantCulture);
                    }

                    if (IsValidRut(Request.Form["rutBuyer"][buyer]) && IsValidOwnershipPercentage(ownershipPercentage) && isValidBuyerOwnershipPercentageSum)
                    {
                        Person newBuyer = new();
                        newBuyer.Rut = Request.Form["rutBuyer"][buyer];
                        newBuyer.OwnershipPercentage= ownershipPercentage;
                        newBuyer.UncreditedOwnership = bool.Parse(Request.Form["uncreditedClickedBuyer"][buyer]);
                        newBuyer.Seller = false;
                        newBuyer.Heir = true;
                        newBuyer.FormsId = getLastFormsRecord(_context).AttentionNumber;
                        _context.Add(newBuyer);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        List<Person> peopleToRemoveFromDb = _context.People.Where(people => people.FormsId == getLastFormsRecord(_context).AttentionNumber).ToList();
                        _context.RemoveRange(peopleToRemoveFromDb);
                        await _context.SaveChangesAsync();
                        _context.Remove(realStateForm);
                        await _context.SaveChangesAsync();
                        ViewBag.Communes = _context.Commune.ToList();
                        return View(realStateForm);
                    }
                }


                await MultiOwnerTableUpdate(_context);
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

        static bool IsValidRut(string rut)
        {
            if (string.IsNullOrEmpty(rut))
            {
                return false;
            }
            return true;
        }

        static bool IsValidOwnershipPercentage(double? ownershipPercentage)
        {
            if (ownershipPercentage < 0 || ownershipPercentage > 100)
            {
                return false;
            }
            return true;
        }

        static bool IsValidBuyersOwnershipPercentageSum(List<string?> buyersOwnershipPercentage)
        {
            buyersOwnershipPercentage.RemoveAt(0);
            double? sumOwnershipPercentage = 0;
            foreach (var buyerOwnershipPercentage in buyersOwnershipPercentage)
            {
                System.Diagnostics.Debug.WriteLine("*****");
                System.Diagnostics.Debug.WriteLine(buyerOwnershipPercentage);
                System.Diagnostics.Debug.WriteLine("*****");
                if (string.IsNullOrEmpty(buyerOwnershipPercentage))
                {
                    sumOwnershipPercentage += 0;
                }
                else
                {
                    sumOwnershipPercentage += double.Parse(buyerOwnershipPercentage, CultureInfo.InvariantCulture);
                }
            }

            if (sumOwnershipPercentage > 100)
            {
                return false;
            }
            return true;
        }

        static async Task MultiOwnerTableUpdate(InscripcionesBrDbContext _context)
        {
            RealStateForm currentForm = getLastFormsRecord(_context);
            bool addToTable = true;
            int adjustedYear = adjustYear(currentForm.InscriptionDate.Year);

            if (checkYearAlreadyExists(currentForm,_context))
            {
                
                List<MultiOwner> latestMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.InscriptionNumber < currentForm.InscriptionNumber &&
                                             multiowner.ValidityYearBegin == adjustedYear && multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                 multiowner.Property == currentForm.Property).ToList();
                System.Diagnostics.Debug.WriteLine(latestMultiOwners.Count());

                if (latestMultiOwners.Count() >= 0)
                {
                    _context.MultiOwners.RemoveRange(latestMultiOwners);
                }
                else { 
                    addToTable = false; 
                }
            }
            if (addToTable)
            {
                List<Person> buyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false).ToList();
                MultiOwner ? nextBuyer = _context.MultiOwners.Where(multiowner => multiowner.ValidityYearBegin > adjustedYear &&
                                        multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                        multiowner.Property == currentForm.Property)
                    .OrderBy(tableKey => tableKey.ValidityYearBegin).LastOrDefault();

                List<Person> uncreditedOwnershipBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false && s.UncreditedOwnership == true).ToList();
                List<Person> creditedOwnershipBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false && s.UncreditedOwnership == false).ToList();
                double? sumCreditedOwnershipPercentage = 0;
                foreach (var creditedPercentageBuyer in creditedOwnershipBuyers)
                {
                    sumCreditedOwnershipPercentage += creditedPercentageBuyer.OwnershipPercentage;
                }

                double? ownershipPercentageToAssign = (100 - sumCreditedOwnershipPercentage)/uncreditedOwnershipBuyers.Count;

                foreach (var uncreditedOwnershipBuyer in uncreditedOwnershipBuyers) 
                {
                    uncreditedOwnershipBuyer.OwnershipPercentage = ownershipPercentageToAssign;

                }

                foreach (var buyer in buyers)
                {
                    MultiOwner newMultiOwner = new MultiOwner();
                    newMultiOwner.Rut = buyer.Rut;
                    newMultiOwner.Sheets = currentForm.Sheets;
                    newMultiOwner.OwnershipPercentage = buyer.OwnershipPercentage;
                    newMultiOwner.ValidityYearBegin = adjustedYear;
                    newMultiOwner.Commune = currentForm.Commune;
                    newMultiOwner.Block = currentForm.Block;
                    newMultiOwner.Property = currentForm.Property;
                    newMultiOwner.InscriptionDate = currentForm.InscriptionDate;
                    newMultiOwner.InscriptionNumber = currentForm.InscriptionNumber;
                    if (nextBuyer != null)
                    {
                        newMultiOwner.ValidityYearFinish = nextBuyer.ValidityYearBegin - 1;
                        _context.Add(newMultiOwner);
                       
                    }
                    else { 
                        newMultiOwner.ValidityYearFinish = null;
                        _context.Add(newMultiOwner);
                    }
                }
                await _context.SaveChangesAsync();

                List<MultiOwner> previousMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.ValidityYearFinish == null
                                                && multiowner.ValidityYearBegin < adjustedYear && multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                 multiowner.Property == currentForm.Property).ToList();
                foreach (var previousMultiOwner  in previousMultiOwners)
                {
                    previousMultiOwner.ValidityYearFinish = adjustedYear - 1;
                    _context.Update(previousMultiOwner);
                    await _context.SaveChangesAsync();
                }
            }
        }

        static RealStateForm getLastFormsRecord(InscripcionesBrDbContext _context)
        {
           return  _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault();
        }

        static bool checkYearAlreadyExists(RealStateForm realStateForm, InscripcionesBrDbContext _context)
        {
            int year = adjustYear(realStateForm.InscriptionDate.Year);  
            System.Diagnostics.Debug.WriteLine(year);
            List<MultiOwner> multiOwnersOfGivenYear = _context.MultiOwners.Where(multiowner => multiowner.InscriptionDate.Year == year &&
                                                 multiowner.Block == realStateForm.Block && multiowner.Commune == realStateForm.Commune &&
                                                 multiowner.Property == realStateForm.Property).ToList();
            if (multiOwnersOfGivenYear.Count()>=0)
            {
                return true;
            }
            return false;
        }

        static int adjustYear(int year)
        {
            int adjustedYear = year;
            if (year < 2019) {
                adjustedYear = 2019;
            }
            return adjustedYear;
        }
    }
}

