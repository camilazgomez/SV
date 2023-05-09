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
        private IFormCollection form;
        private const string standardPatrimonyRegularisation = "Regularización de Patrimonio";

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
            bool invalidForm = id == null || _context.RealStateForms == null;
            if (invalidForm)
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
            form = Request.Form;
            bool saveForm = ModelState.IsValid && form["NatureOfTheDeed"] == standardPatrimonyRegularisation;
            if (saveForm)
            {
                _context.Add(realStateForm);
                await _context.SaveChangesAsync();

                if (form["NatureOfTheDeed"] != standardPatrimonyRegularisation)
                {
                    bool validSellers = AreValidFormSellers(form);
                    if (validSellers)
                    {
                        await AddSellers(_context, form);
                    }
                    else
                    {
                        _context.Remove(realStateForm);
                        await _context.SaveChangesAsync();
                        ViewBag.Communes = _context.Commune.ToList();
                        return View(realStateForm);
                    } 
                }
                List<string?> buyersOwnershipPercentage = Request.Form["ownershipPercentageBuyer"].ToList();
                bool isValidBuyerOwnershipPercentageSum = IsValidBuyersOwnershipPercentageSum(buyersOwnershipPercentage);
                bool validBuyers = AreValidFormBuyers(form);
                if (validBuyers && isValidBuyerOwnershipPercentageSum)
                {
                    await AddBuyers(_context, form);
                }
                else
                {
                    List<Person> peopleToRemoveFromDb = _context.People.Where(people => people.FormsId == GetLastFormsRecord(_context).AttentionNumber).ToList();
                    _context.RemoveRange(peopleToRemoveFromDb);
                    await _context.SaveChangesAsync();
                    _context.Remove(realStateForm);
                    await _context.SaveChangesAsync();
                    ViewBag.Communes = _context.Commune.ToList();
                    return View(realStateForm);
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
            bool invalidForm = id == null || _context.RealStateForms == null;
            if (invalidForm)
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

        private static bool AreValidFormSellers(IFormCollection form)
        {
            for (int seller = 1; seller < form["rutSeller"].Count; seller++)
            {
                double? ownershipPercentage;
                bool uncreditedPercentage = bool.Parse(form["uncreditedClickedSeller"][seller]);
                ownershipPercentage = getOwnershipPercentage(uncreditedPercentage, form["ownershipPercentageSeller"][seller]);
                bool validInputSeller = IsValidRut(form["rutSeller"][seller]) && IsValidOwnershipPercentage(ownershipPercentage);
                if (!validInputSeller)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AreValidFormBuyers(IFormCollection form)
        {
            for (int buyer = 1; buyer < form["rutBuyer"].Count; buyer++)
            {
                double? ownershipPercentage;
                bool uncreditedClickedBuyer = bool.Parse(form["uncreditedClickedBuyer"][buyer]);
                if (string.IsNullOrEmpty(form["ownershipPercentageBuyer"][buyer]))
                {
                    uncreditedClickedBuyer = true;
                }
                ownershipPercentage = getOwnershipPercentage(uncreditedClickedBuyer, form["ownershipPercentageBuyer"][buyer]);
                bool validBuyerInput = IsValidRut(form["rutBuyer"][buyer]) && IsValidOwnershipPercentage(ownershipPercentage);
                if (!validBuyerInput)
                {
                    return false;
                }
            }
            return true;
        }

        private static double? getOwnershipPercentage(bool uncreditedPercentage, string ownershipPercentageFromForm)
        {
            double? ownershipPercentage;
            if (uncreditedPercentage)
            {
                ownershipPercentage = null;
            }
            else
            {
                ownershipPercentage = double.Parse(ownershipPercentageFromForm, CultureInfo.InvariantCulture);
            }
            return ownershipPercentage;
        }

        private static bool IsValidRut(string rut)
        {
            if (string.IsNullOrEmpty(rut))
            {
                return false;
            }
            return true;
        }

        private static bool IsValidOwnershipPercentage(double? ownershipPercentage)
        {
            bool validPercentage = ownershipPercentage < 0 || ownershipPercentage > 100;
            if (validPercentage)
            {
                return false;
            }
            return true;
        }

        private static bool IsValidBuyersOwnershipPercentageSum(List<string?> buyersOwnershipPercentage)
        {
            buyersOwnershipPercentage.RemoveAt(0);
            double? sumOwnershipPercentage = 0;
            foreach (var buyerOwnershipPercentage in buyersOwnershipPercentage)
            {
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

        private static async Task AddSellers(InscripcionesBrDbContext _context, IFormCollection form)
        {
            for (int seller = 1; seller < form["rutSeller"].Count; seller++)
            {
                double? ownershipPercentage;
                bool uncreditedPercentage = bool.Parse(form["uncreditedClickedSeller"][seller]);
                ownershipPercentage = getOwnershipPercentage(uncreditedPercentage, form["ownershipPercentageSeller"][seller]);
                Person newSeller = new(form["rutSeller"][seller], ownershipPercentage, bool.Parse(form["uncreditedClickedSeller"][seller]), GetLastFormsRecord(_context).AttentionNumber, true, false);
                _context.Add(newSeller);
                await _context.SaveChangesAsync();
            }
        }

        private static async Task AddBuyers(InscripcionesBrDbContext _context, IFormCollection form)
        {
            for (int buyer = 1; buyer < form["rutBuyer"].Count; buyer++)
            {
                double? ownershipPercentage;
                bool uncreditedClickedBuyer = bool.Parse(form["uncreditedClickedBuyer"][buyer]);
                if (string.IsNullOrEmpty(form["ownershipPercentageBuyer"][buyer]))
                {
                    uncreditedClickedBuyer = true;
                }
                ownershipPercentage = getOwnershipPercentage(uncreditedClickedBuyer, form["ownershipPercentageBuyer"][buyer]);
                Person newBuyer = new(form["rutBuyer"][buyer], ownershipPercentage, uncreditedClickedBuyer, GetLastFormsRecord(_context).AttentionNumber, false, true);
                newBuyer.Rut = form["rutBuyer"][buyer];
                _context.Add(newBuyer);
                await _context.SaveChangesAsync();
            }
        }

        private static async Task MultiOwnerTableUpdate(InscripcionesBrDbContext _context)
        {
            RealStateForm currentForm = GetLastFormsRecord(_context);
            bool addToTable = true;
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);

            if (CheckYearAlreadyExists(currentForm,_context))
            {
                List<MultiOwner> higherInscriptionNumberMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.InscriptionNumber > currentForm.InscriptionNumber &&
                                             multiowner.ValidityYearBegin == adjustedYear && multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                 multiowner.Property == currentForm.Property).ToList();
                List<MultiOwner> previousMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.InscriptionNumber < currentForm.InscriptionNumber &&
                                             multiowner.ValidityYearBegin == adjustedYear && multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                 multiowner.Property == currentForm.Property).ToList();
                bool latestmultiOwner = previousMultiOwners.Count() >= 0 && higherInscriptionNumberMultiOwners.Count() == 0;
                if (latestmultiOwner)
                {
                    _context.MultiOwners.RemoveRange(previousMultiOwners);
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
                    if (nextBuyer != null)
                    {
                        int validityYearFinish = nextBuyer.ValidityYearBegin - 1;
                        MultiOwner newMultiOwner = new MultiOwner(buyer.Rut, buyer.OwnershipPercentage, currentForm.Commune, currentForm.Block, currentForm.Property, currentForm.Sheets, currentForm.InscriptionDate,
                                                currentForm.InscriptionNumber, adjustedYear, validityYearFinish);
                        _context.Add(newMultiOwner);
                       
                    }
                    else { 
                        int? validityYearFinish = null;
                        MultiOwner newMultiOwner = new MultiOwner(buyer.Rut, buyer.OwnershipPercentage, currentForm.Commune, currentForm.Block, currentForm.Property, currentForm.Sheets, currentForm.InscriptionDate,
                                                currentForm.InscriptionNumber, adjustedYear, validityYearFinish);
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

        private static RealStateForm GetLastFormsRecord(InscripcionesBrDbContext _context)
        {
           return  _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault();
        }

        private static bool CheckYearAlreadyExists(RealStateForm realStateForm, InscripcionesBrDbContext _context)
        {
            int year = AdjustYear(realStateForm.InscriptionDate.Year);  
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

        private static int AdjustYear(int year)
        {
            int adjustedYear = year;
            if (year < 2019) {
                adjustedYear = 2019;
            }
            return adjustedYear;
        }
    }
}

