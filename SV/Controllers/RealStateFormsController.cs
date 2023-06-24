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
using System.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using System.Diagnostics.Metrics;
using Newtonsoft.Json.Linq;

namespace SV.Controllers
{
    public class RealStateFormsController : Controller
    {
        private readonly InscripcionesBrDbContext _context;
        private const string standardPatrimonyRegularisation = "Regularización de Patrimonio";
        private const int limitYear = 2019;
        private const int pairIdentifier = 2;
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

            var realStateForm = _context.RealStateForms != null ? await _context.RealStateForms
                .FirstOrDefaultAsync(m => m.AttentionNumber == id): null;
            if (realStateForm == null)
            {
                return NotFound();
            }
            ExpandedDetailsOfForms viewData = new(_context.People.Where(s => s.FormsId == id && s.Seller == true).ToList(),
                _context.People.Where(s => s.FormsId == id && s.Seller == false).ToList(), realStateForm);

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
            IFormCollection form = Request.Form;
            bool saveForm = ModelState.IsValid;
            if (saveForm && IsValidInscriptionDate(form["InscriptionDate"]))
            {
                realStateForm.Valid = true;
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
                    RealStateForm? lastRecord = GetLastFormsRecord(_context);
                    if (lastRecord != null)
                    {
                        List<Person> peopleToRemoveFromDb = _context.People.Where(people => people.FormsId == lastRecord.AttentionNumber).ToList();
                        _context.RemoveRange(peopleToRemoveFromDb);
                        await _context.SaveChangesAsync();
                        _context.Remove(realStateForm);
                        await _context.SaveChangesAsync();
                        ViewBag.Communes = _context.Commune.ToList();
                        return View(realStateForm);
                    }
                    
                }
                RealStateForm? currentForm = GetLastFormsRecord(_context);
                if (currentForm != null)
                {
                    await ManageRepetedForms(_context, currentForm);
                    return RedirectToAction(nameof(Index));
                }
               
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

            var realStateForm = _context.RealStateForms != null ? await _context.RealStateForms
                .FirstOrDefaultAsync(m => m.AttentionNumber == id) : null;
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
                RealStateForm? newValidForm = _context.RealStateForms.Where(e => e.InscriptionDate.Year == realStateForm.InscriptionDate.Year
                                            && e.InscriptionNumber == realStateForm.InscriptionNumber &&
                                            e.Valid == false).OrderByDescending(e => e.AttentionNumber).FirstOrDefault();
                List<Person> peopleToBeRemoved = _context.People.Where(e => e.FormsId == realStateForm.AttentionNumber).ToList();
                _context.RemoveRange(peopleToBeRemoved);
                _context.RealStateForms.Remove(realStateForm);
                await _context.SaveChangesAsync();
                if (newValidForm != null)
                {
                    newValidForm.Valid = true;
                    _context.Update(newValidForm);
                }
                
                List<MultiOwner> multiOwners = _context.MultiOwners.ToList();
                _context.MultiOwners.RemoveRange(multiOwners);
                await _context.SaveChangesAsync();
                List<RealStateForm> allForms = _context.RealStateForms.Where(e => e.Valid == true).OrderBy(e => e.AttentionNumber).ToList();
                
                foreach (var form in allForms)
                {
                    await MultiOwnerTableUpdate(_context, form);
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


            private static async Task ManageRepetedForms(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            List<RealStateForm> repetedForms = _context.RealStateForms.Where(e => e.InscriptionNumber == currentForm.InscriptionNumber
                                                && e.InscriptionDate.Year == currentForm.InscriptionDate.Year &&
                                                e.AttentionNumber != currentForm.AttentionNumber).ToList();
            foreach (var form in repetedForms)
            {
                form.Valid = false;
                _context.Update(form);
            }
            await _context.SaveChangesAsync();
            List<RealStateForm> allForms = _context.RealStateForms.Where(e => e.Valid == true).OrderBy(e => e.AttentionNumber).ToList();
            List<MultiOwner> multiOwners = _context.MultiOwners.ToList();
            _context.MultiOwners.RemoveRange(multiOwners);
            await _context.SaveChangesAsync();
            foreach (var form in allForms)
            {
                await MultiOwnerTableUpdate(_context, form);
            }
        }

        private bool RealStateFormExists(int id)
        {
            return (_context.RealStateForms?.Any(e => e.AttentionNumber == id)).GetValueOrDefault();
        }

        private static bool AreValidFormSellers(IFormCollection form)
        {   
            if (form["rutSeller"].Count == 0)
            {
                return false;
            }
            for (int seller = 1; seller < form["rutSeller"].Count; seller++)
            {
                double? ownershipPercentage;
                bool uncreditedPercentage;
                if (form["uncreditedClickedSeller"][seller] != null)
                {
                    string? uncreditedPercentageStr = form["uncreditedClickedSeller"][seller];
                    uncreditedPercentage = uncreditedPercentageStr != null && bool.Parse(uncreditedPercentageStr);
                }
                else
                {
                    uncreditedPercentage = false;
                }
                
                ownershipPercentage = GetOwnershipPercentage(uncreditedPercentage, form["ownershipPercentageSeller"][seller]);
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
                string? uncreditedClickedBuyerStr = form["uncreditedClickedBuyer"][buyer];
                bool uncreditedClickedBuyer = uncreditedClickedBuyerStr != null && bool.Parse(uncreditedClickedBuyerStr);
                if (string.IsNullOrEmpty(form["ownershipPercentageBuyer"][buyer]))
                {
                    uncreditedClickedBuyer = true;
                }
                ownershipPercentage = GetOwnershipPercentage(uncreditedClickedBuyer, form["ownershipPercentageBuyer"][buyer]);
                bool validBuyerInput = IsValidRut(form["rutBuyer"][buyer]) && IsValidOwnershipPercentage(ownershipPercentage);
                if (!validBuyerInput)
                {
                    return false;
                }
            }
            return true;
        }

        private static double? GetOwnershipPercentage(bool uncreditedPercentage, string? ownershipPercentageFromForm)
        {
            double? ownershipPercentage;
            if (uncreditedPercentage)
            {
                ownershipPercentage = null;
            }
            else
            {
                if (ownershipPercentageFromForm != null)
                {
                    ownershipPercentage = double.Parse(ownershipPercentageFromForm, CultureInfo.InvariantCulture);
                }
                else
                {
                    ownershipPercentage = null;
                }
                
            }
            return ownershipPercentage;
        }

        private static async Task AddSellers(InscripcionesBrDbContext _context, IFormCollection form)
        {
            for (int seller = 1; seller < form["rutSeller"].Count; seller++)
            {
                double? ownershipPercentage;
                string? uncreditedPercentageStr = form["uncreditedClickedSeller"][seller];
                bool uncreditedPercentage = uncreditedPercentageStr != null && bool.Parse(uncreditedPercentageStr);
                ownershipPercentage = GetOwnershipPercentage(uncreditedPercentage, form["ownershipPercentageSeller"][seller]);
                RealStateForm? lastForm = GetLastFormsRecord(_context);

                if (lastForm != null)
                {
                    Person newSeller = new(form["rutSeller"][seller], ownershipPercentage, uncreditedPercentage, lastForm.AttentionNumber, true, false);
                    _context.Add(newSeller);
                    await _context.SaveChangesAsync();

                }
            }
        }

        private static async Task AddBuyers(InscripcionesBrDbContext _context, IFormCollection form)
        {
            for (int buyer = 1; buyer < form["rutBuyer"].Count; buyer++)
            {
                double? ownershipPercentage;
                string? uncreditedClickedBuyerStr = form["uncreditedClickedBuyer"][buyer];
                bool uncreditedClickedBuyer = uncreditedClickedBuyerStr != null && bool.Parse(uncreditedClickedBuyerStr);
                if (string.IsNullOrEmpty(form["ownershipPercentageBuyer"][buyer]))
                {
                    uncreditedClickedBuyer = true;
                }
                ownershipPercentage = GetOwnershipPercentage(uncreditedClickedBuyer, form["ownershipPercentageBuyer"][buyer]);
                RealStateForm? lastForm = GetLastFormsRecord(_context);
                if (lastForm != null )
                {
                    Person newBuyer = new(form["rutBuyer"][buyer], ownershipPercentage, uncreditedClickedBuyer, lastForm.AttentionNumber, false, true)
                    {
                        Rut = form["rutBuyer"][buyer]
                    };
                    _context.Add(newBuyer);
                    await _context.SaveChangesAsync();
                }
            }
        }

        private static async Task MultiOwnerTableUpdate(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            bool addToTable = true;
            if (CheckYearAlreadyExists(currentForm, _context) && currentForm.NatureOfTheDeed == standardPatrimonyRegularisation)
            {
                List<MultiOwner> higherInscriptionNumberMultiOwners = GetMultiOwnersWithHigherInscriptionNumber(_context, currentForm);
                List<MultiOwner> previousMultiOwners = GetMultiOwnersWithLowerInscriptionNumber(_context, currentForm);
                bool latestmultiOwner = previousMultiOwners.Count >= 0 && higherInscriptionNumberMultiOwners.Count == 0;
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
                if (currentForm.NatureOfTheDeed == standardPatrimonyRegularisation)
                {
                    await ManageRPD(_context, currentForm);
                }
                else
                {
                    await ManageCompraventa(_context, currentForm);
                }
            }
        }
        private static async Task ManageRPD(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            List<Person> originalBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false).ToList();
            List<Person> sellers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true).ToList();
            List<Person> buyers = new();
            List<Person> uncreditedOwnershipBuyers = new();
            List<Person> creditedOwnershipBuyers = new();

            foreach (var buyer in originalBuyers)
            {
                Person copyBuyer = new(buyer.Rut, buyer.OwnershipPercentage, buyer.UncreditedOwnership,
                                    currentForm.AttentionNumber, false, true);
                _context.Add(copyBuyer);
                await _context.SaveChangesAsync();
                buyers.Add(copyBuyer);

                if (copyBuyer.UncreditedOwnership == true)
                {
                    uncreditedOwnershipBuyers.Add(copyBuyer);
                }
                else
                {
                    creditedOwnershipBuyers.Add(copyBuyer);
                }
            }
            double? ownershipPercentageToAssign = GetAssignedOwnershipPercentage(creditedOwnershipBuyers, uncreditedOwnershipBuyers);
            foreach (var uncreditedOwnershipBuyer in uncreditedOwnershipBuyers)
            {
                uncreditedOwnershipBuyer.OwnershipPercentage = ownershipPercentageToAssign;

            }
            await AddNewMultiOwners(_context, buyers, currentForm);
            await SetFinalYearPreviousMultiOwners(_context, currentForm);
            _context.RemoveRange(buyers);
            await _context.SaveChangesAsync();
        }

        private static async Task ManageCompraventa(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            List<Person> originalBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false).ToList();
            List<Person> buyers = new();
            foreach (var buyer in originalBuyers)
            {
                Person copyBuyer = new(buyer.Rut, buyer.OwnershipPercentage, buyer.UncreditedOwnership,
                                    currentForm.AttentionNumber, false, true);
                _context.Add(copyBuyer);
                await _context.SaveChangesAsync();
                buyers.Add(copyBuyer);
            }

            List<Person> sellers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true).ToList();
            double? totalBuyersSum = buyers.Sum(item => item.OwnershipPercentage);
            List<string?> ruts = sellers.Select(o => o.Rut).ToList();
            List<MultiOwner> allOwnersForPeriod = GetAllOwnersFromPeriod(_context, currentForm);
            List<MultiOwner> sellerMultiOwners = GetSellerOwnersFromPeriod(_context, ruts, currentForm);
            bool totalBuyersSumBetween100And0 = totalBuyersSum < 100 && totalBuyersSum > 0;
            bool oneBuyerAndOneSeller = buyers.Count == 1 && sellers.Count == 1;

            if (totalBuyersSum == 100)
            {
                AssignCompraventaOwnershipPercentage(buyers, sellerMultiOwners);
                await AddNewMultiOwners(_context, buyers, currentForm);
                if (sellerMultiOwners.Count > 0)
                {
                    List<MultiOwner> multiownersToDelete = new();
                    SetMultiOwnersToDelete(ref multiownersToDelete, sellerMultiOwners, adjustedYear);
                    _context.RemoveRange(multiownersToDelete);
                }
            }
            else if (totalBuyersSumBetween100And0 && oneBuyerAndOneSeller)
            {
                AssignCompraventaOwnershipPercentage(buyers, sellerMultiOwners);
                if (sellerMultiOwners.Count > 0)
                {
                    double? updatedOwnershipSeller = sellerMultiOwners[0].OwnershipPercentage - sellerMultiOwners[0].OwnershipPercentage * sellers[0].OwnershipPercentage / 100;
                    MultiOwner sellerMO = sellerMultiOwners[0];
                    Person seller = sellers[0];
                    SetMultiOwnerRecords(adjustedYear, ref sellerMO, updatedOwnershipSeller, ref seller, ref buyers);
                }
                else
                {
                    CreateGhostOwner(_context, currentForm, sellers[0].Rut);
                }
                
                await AddNewMultiOwners(_context, buyers, currentForm);
            }
            else
            {
                await ProcessDomainSale(_context, buyers, currentForm, ruts);
            }
            await OldRecordKeeping(_context, currentForm, allOwnersForPeriod, sellerMultiOwners, adjustedYear);
            await MergeDuplicateRegisters(_context, currentForm, adjustedYear, ruts);
            await SetNegativePercentagesToZero(_context, currentForm);
            await AdjustPercentages(_context, currentForm);
            List<MultiOwner> zeroOwnershipMultiOwners = GetOwnersWithNoPercentage(_context, currentForm);
            _context.RemoveRange(zeroOwnershipMultiOwners);
            _context.RemoveRange(buyers);
            await _context.SaveChangesAsync();
        }

        private static async Task ProcessDomainSale(InscripcionesBrDbContext _context, List<Person> buyers, RealStateForm currentForm, List<string?> ruts)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            foreach (var rut in ruts)
            {
                if (rut != null)
                {
                    Person? currentSeller = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true && s.Rut == rut).
                                            OrderBy(tableKey => tableKey.Id).LastOrDefault();
                    MultiOwner? currentMultiOwner = GetOwnerRecordByRut(_context, rut, currentForm);
                    if (currentMultiOwner != null && currentSeller != null)
                    {
                        double? newOwnershipPercentage = currentMultiOwner.OwnershipPercentage - currentSeller.OwnershipPercentage;
                        SetMultiOwnerRecords(adjustedYear, ref currentMultiOwner, newOwnershipPercentage, ref currentSeller, ref buyers);
                    }
                    else
                    {
                        CreateGhostOwner(_context, currentForm, rut);
                    }
                }
            }
            await AddNewMultiOwners(_context, buyers, currentForm);

        }

            private static async Task AddNewMultiOwners(InscripcionesBrDbContext _context, List<Person> buyers, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            MultiOwner? nextBuyer = FindNextOwner(_context, currentForm);
            foreach (var buyer in buyers)
            {
                if (nextBuyer != null)
                {
                    int validityYearFinish = nextBuyer.ValidityYearBegin - 1;
                    MultiOwner newMultiOwner = new(buyer.Rut, buyer.OwnershipPercentage,
                                            currentForm.Commune, currentForm.Block, currentForm.Property,
                                            currentForm.Sheets, currentForm.InscriptionDate,
                                            currentForm.InscriptionNumber, adjustedYear, validityYearFinish);
                    _context.Add(newMultiOwner);

                }
                else
                {
                    int? validityYearFinish = null;
                    MultiOwner newMultiOwner = new(buyer.Rut, buyer.OwnershipPercentage, currentForm.Commune,
                                             currentForm.Block, currentForm.Property, currentForm.Sheets, currentForm.InscriptionDate,
                                             currentForm.InscriptionNumber, adjustedYear, validityYearFinish);
                    _context.Add(newMultiOwner);
                }
            }
            await _context.SaveChangesAsync();
        }

        private static async Task SetFinalYearPreviousMultiOwners(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            List<MultiOwner> previousMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.ValidityYearFinish == null
                                               && multiowner.ValidityYearBegin < adjustedYear && multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                multiowner.Property == currentForm.Property).ToList();
            foreach (var previousMultiOwner in previousMultiOwners)
            {
                previousMultiOwner.ValidityYearFinish = adjustedYear - 1;
                _context.Update(previousMultiOwner);
                await _context.SaveChangesAsync();
            }
        }

        private static List<MultiOwner> GetOwnersWithNoPercentage(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            List<MultiOwner> ownersWithNoPercentage = _context.MultiOwners.Where(multiowner =>  multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                                multiowner.Property == currentForm.Property && multiowner.OwnershipPercentage == 0).ToList();
            return ownersWithNoPercentage;
        }

        private static RealStateForm? GetLastFormsRecord(InscripcionesBrDbContext _context)
        {
            return _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault();
        }

        private static void AssignCompraventaOwnershipPercentage(List<Person> buyers, List<MultiOwner> sellers)
        {
            int ghostSellers = sellers.Count;
            double? totalSellersSum = 0; 
            if (ghostSellers == 0)
            {
                totalSellersSum = 100;
            }
            else
            {
                totalSellersSum = sellers?.Sum(m => m.OwnershipPercentage);
            }
            foreach (var buyer in buyers)
            {
                buyer.OwnershipPercentage = totalSellersSum * (buyer.OwnershipPercentage / 100);
            }
        }

        private static async Task MergeDuplicateRegisters(InscripcionesBrDbContext _context, RealStateForm currentForm, int adjustedYear, List<string?> ruts)
        {
            List<MultiOwner> allMultiOwners = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                              item.Block == currentForm.Block && item.Commune == currentForm.Commune)
                                              .ToList();
            List<String> newRuts = new();
            List<int> beginValidities = new();

            foreach (var multiowner in allMultiOwners)
            {
                List<MultiOwner> multiownersWithSameRut = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.Rut == multiowner.Rut && item.ValidityYearBegin == multiowner.ValidityYearBegin)
                                                    .ToList();
                if (multiownersWithSameRut.Count > 1 && !ruts.Contains(multiowner.Rut))
                {
                    if (multiowner.Rut != null)
                    {
                        newRuts.Add(multiowner.Rut);
                        beginValidities.Add(multiowner.ValidityYearBegin);
                    }

                }
            }
            List<string> filteredRuts = newRuts
                            .Where((s, index) => index % pairIdentifier == 0) 
                            .ToList();
            List<int> filteredYears = beginValidities
                            .Where((s, index) => index % pairIdentifier == 0)
                            .ToList();
            int counter = 0;

            foreach (var rut in filteredRuts)
            {
                MultiOwner? multiOwnerHighestNumber = _context.MultiOwners.Where(item => item.Rut == rut && item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune && 
                                                    item.ValidityYearBegin == filteredYears[counter]).
                                                    OrderByDescending(item => item.InscriptionNumber).FirstOrDefault();

                if (multiOwnerHighestNumber != null)
                {
                    List<MultiOwner> multiownersWithSameRut = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                           item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                           && item.Rut == rut && item.ValidityYearBegin == filteredYears[counter]).ToList();
                    List<MultiOwner> multiownersToDelete = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                        item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                        && item.Rut == rut && item.ValidityYearBegin == multiOwnerHighestNumber.ValidityYearBegin &&
                                                        item.InscriptionNumber != multiOwnerHighestNumber.InscriptionNumber).ToList();
                    double? mergeOwnership = multiownersWithSameRut.Sum(item => item.OwnershipPercentage);
                    MultiOwner multiOwnerToMerge = new(multiOwnerHighestNumber.Rut, mergeOwnership,currentForm.Commune, currentForm.Block,
                                                    currentForm.Property,currentForm.Sheets, currentForm.InscriptionDate,
                                                    multiOwnerHighestNumber.InscriptionNumber, adjustedYear, multiOwnerHighestNumber.ValidityYearFinish);

                    _context.Add(multiOwnerToMerge);
                    _context.RemoveRange(multiownersWithSameRut);
                    counter++;
                }  
            }
            await _context.SaveChangesAsync();
        }

        private static async Task OldRecordKeeping(InscripcionesBrDbContext _context, RealStateForm currentForm, List<MultiOwner> allOwnersForPeriod, List<MultiOwner> sellerMultiOwners, int adjustedYear)
        {
            foreach (var owner in allOwnersForPeriod)
            {
                if (!sellerMultiOwners.Contains(owner) && owner.ValidityYearBegin != adjustedYear)
                {
                    MultiOwner previousMultiOwner = new(owner.Rut, owner.OwnershipPercentage,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, owner.InscriptionDate,
                                    owner.InscriptionNumber, owner.ValidityYearBegin, adjustedYear - 1);
                    owner.ValidityYearBegin = adjustedYear;
                    _context.Add(previousMultiOwner);
                }
            }
            await _context.SaveChangesAsync();
        }

        private static void CreateGhostOwner(InscripcionesBrDbContext _context, RealStateForm currentForm, string? rut)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            MultiOwner ghostOwner = new(rut, 0,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    null, DateTime.MinValue,
                                    null, adjustedYear, null)
            {
                InscriptionDate = DateTime.MinValue
            };
            _context.Add(ghostOwner);
        }

        private static void SetMultiOwnerRecords(int adjustedYear, ref MultiOwner multiOwner, double? ownershipPercentage, ref Person seller, ref List<Person> buyers)
        {
            if (multiOwner.ValidityYearBegin == adjustedYear)
            {
                multiOwner.OwnershipPercentage = ownershipPercentage;
            }
            else
            {
                multiOwner.ValidityYearFinish = adjustedYear - 1;
                seller.OwnershipPercentage = ownershipPercentage;
                buyers.Add(seller);
            }

        }

        private static void SetMultiOwnersToDelete(ref List<MultiOwner> multiownersToDelete, List<MultiOwner> sellerMultiOwners, int adjustedYear)
        {
            foreach (var sellerMultiOwner in sellerMultiOwners)
            {
                if (sellerMultiOwner.ValidityYearBegin < adjustedYear)
                {
                    sellerMultiOwner.ValidityYearFinish = adjustedYear - 1;
                }
                else
                {
                    multiownersToDelete.Add(sellerMultiOwner);
                }
            }
        }


        private static async Task SetNegativePercentagesToZero(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            List<MultiOwner> negativeOwnershipMultiOwners = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune &&
                                                    item.OwnershipPercentage < 0)
                                                    .ToList();
            foreach (var negativeOwner in negativeOwnershipMultiOwners)
            {
                MultiOwner correctedMultiOwner = new(negativeOwner.Rut, 0,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    negativeOwner.InscriptionNumber, negativeOwner.ValidityYearBegin, negativeOwner.ValidityYearFinish);
                _context.Add(correctedMultiOwner);
            }
            _context.RemoveRange(negativeOwnershipMultiOwners);
            await _context.SaveChangesAsync();
        }


        private static async Task AdjustPercentages(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            List<MultiOwner> zeroOwnershipMultiOwners = GetOwnersWithNoPercentage(_context, currentForm);
            List<MultiOwner> sameYearMultiOwners = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.ValidityYearBegin <= adjustedYear &&
                                                    (item.ValidityYearFinish == null || item.ValidityYearFinish > adjustedYear))
                                                    .ToList();
            double? sumOfOwnerships = sameYearMultiOwners.Sum(item => item.OwnershipPercentage);
            int numberOfOwners = zeroOwnershipMultiOwners.Count;
            if (sumOfOwnerships > 100)
            {
                double? ratio = 100 / sumOfOwnerships;
                foreach (var owner in sameYearMultiOwners)
                {
                    MultiOwner correctedMultiOwner = new(owner.Rut, owner.OwnershipPercentage * ratio,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    owner.InscriptionNumber, owner.ValidityYearBegin, owner.ValidityYearFinish);
                    _context.Add(correctedMultiOwner);
                }
                _context.RemoveRange(sameYearMultiOwners);
            }
            else if (sumOfOwnerships < 100 && numberOfOwners > 0)
            {
                foreach (var owner in zeroOwnershipMultiOwners)
                {
                    
                    double? partedOwnership = (100-sumOfOwnerships) / numberOfOwners;
                    MultiOwner correctedMultiOwner = new(owner.Rut, partedOwnership,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    owner.InscriptionNumber, owner.ValidityYearBegin, owner.ValidityYearFinish);
                    _context.Add(correctedMultiOwner);
                    _context.RemoveRange(zeroOwnershipMultiOwners);
                }
            }
            await _context.SaveChangesAsync();
        }

        private static bool CheckYearAlreadyExists(RealStateForm realStateForm, InscripcionesBrDbContext _context)
        {
            int year = AdjustYear(realStateForm.InscriptionDate.Year);  
            System.Diagnostics.Debug.WriteLine(year);
            List<MultiOwner> multiOwnersOfGivenYear = _context.MultiOwners.Where(multiowner => multiowner.InscriptionDate.Year == 
                                     year && multiowner.Block == realStateForm.Block && multiowner.Commune == realStateForm.Commune 
                                     && multiowner.Property == realStateForm.Property).ToList();
            if (multiOwnersOfGivenYear.Count >=0)
            {
                return true;
            }
            return false;
        }

        private static List<MultiOwner> GetMultiOwnersWithHigherInscriptionNumber(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            var queryMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.InscriptionNumber > currentForm.InscriptionNumber &&
                                    multiowner.ValidityYearBegin == adjustedYear && multiowner.Block == currentForm.Block && 
                                    multiowner.Commune == currentForm.Commune &&
                                    multiowner.Property == currentForm.Property).ToList();
            return queryMultiOwners;
        }

        private static List<MultiOwner> GetMultiOwnersWithLowerInscriptionNumber(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            var queryMultiOwners = _context.MultiOwners.Where(multiowner => multiowner.InscriptionNumber < currentForm.InscriptionNumber &&
                                   multiowner.ValidityYearBegin == adjustedYear && multiowner.Block == currentForm.Block &&
                                   multiowner.Commune == currentForm.Commune &&
                                   multiowner.Property == currentForm.Property).ToList();
            return queryMultiOwners;
        }

        private static MultiOwner? FindNextOwner(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            var queryMultiOwner = _context.MultiOwners.Where(multiowner => multiowner.ValidityYearBegin > adjustedYear &&
                                        multiowner.Block == currentForm.Block && multiowner.Commune == currentForm.Commune &&
                                        multiowner.Property == currentForm.Property)
                                        .OrderBy(tableKey => tableKey.ValidityYearBegin).LastOrDefault();
            return queryMultiOwner;
        }

        private static double? GetAssignedOwnershipPercentage(List<Person> creditedOwnershipBuyers, List<Person> uncreditedOwnershipBuyers)
        {
            double? sumCreditedOwnershipPercentage = 0;
            foreach (var creditedPercentageBuyer in creditedOwnershipBuyers)
            {
                sumCreditedOwnershipPercentage += creditedPercentageBuyer.OwnershipPercentage;
            }

            double? ownershipPercentageToAssign = (100 - sumCreditedOwnershipPercentage) / uncreditedOwnershipBuyers.Count;
            return ownershipPercentageToAssign;
        }

        private static MultiOwner? GetOwnerRecordByRut(InscripcionesBrDbContext _context, string rut, RealStateForm currentForm)
        {
            return _context.MultiOwners.Where(m => m.Rut == rut && m.Property == currentForm.Property &&
                                                       m.Block == currentForm.Block && m.Commune == currentForm.Commune &&
                                                       m.ValidityYearFinish == null).
                                                       OrderBy(tableKey => tableKey.Id).LastOrDefault();
        }

        private static List<MultiOwner> GetAllOwnersFromPeriod(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            return _context.MultiOwners.Where(m => m.Property == currentForm.Property &&
                                               m.Block == currentForm.Block && m.Commune == currentForm.Commune &&
                                               m.ValidityYearBegin <= adjustedYear && (m.ValidityYearFinish > adjustedYear ||
                                               m.ValidityYearFinish == null)).ToList(); ;
        }

        private static List<MultiOwner> GetSellerOwnersFromPeriod(InscripcionesBrDbContext _context, List<String?> ruts, RealStateForm currentForm)
        {
            List<MultiOwner> sellerMultiOwners = new();
            foreach (var rut in ruts)
            {
                if (rut != null)
                {
                    MultiOwner? sellerMultiOwner = GetOwnerRecordByRut(_context, rut, currentForm);
                    if (sellerMultiOwner != null)
                    {
                        sellerMultiOwners.Add(sellerMultiOwner);
                    }
                }
            }
            return sellerMultiOwners;
        }

        private static int AdjustYear(int year)
        {
            int adjustedYear = year;
            if (year < limitYear) {
                adjustedYear = limitYear;
            }
            return adjustedYear;
        }

        public static bool IsValidInscriptionDate(string? formDate)
        {
            if (formDate == null)
            {
                return false;
            }
            var today = DateTime.Now;
            var date = DateTime.Parse(formDate);

            int compareResult = DateTime.Compare(today, date);

            if (compareResult < 0)
            {
                return false;
            }
            return true;
        }

        public static bool IsValidRut(string? rut)
        {
            if (string.IsNullOrEmpty(rut))
            {
                return false;
            }
            rut = rut.Replace(".", "").ToUpper();
            Regex expression = new("^([0-9]+-[0-9K])$");
            string dv = rut.Substring(rut.Length - 1, 1);
            if (!expression.IsMatch(rut))
            {
                return false;
            }
            char[] separator = { '-' };
            string[] rutAux = rut.Split(separator);
            if (dv != CalculateVerificatorDigit(int.Parse(rutAux[0])))
            {
                return false;
            }
            return true;
        }

        public static string CalculateVerificatorDigit(int rut)
        {
            int sum = 0;
            int multiplier = 1;
            while (rut != 0)
            {
                multiplier++;
                if (multiplier == 8)
                    multiplier = 2;
                sum += (rut % 10) * multiplier;
                rut /= 10;
            }
            sum = 11 - (sum % 11);
            if (sum == 11)
            {
                return "0";
            }
            else if (sum == 10)
            {
                return "K";
            }
            else
            {
                return sum.ToString();
            }
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
    }
}

