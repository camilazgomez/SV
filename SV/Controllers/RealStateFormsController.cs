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
            viewData.Sellers = _context.People.Where(s => s.FormsId == id && s.Seller == true).ToList();
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
            bool saveForm = ModelState.IsValid;
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
                //await CleanMultiOwnerTable(_context);
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
                bool uncreditedClickedBuyer = bool.Parse(form["uncreditedClickedBuyer"][buyer]);
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

        private static double? GetOwnershipPercentage(bool uncreditedPercentage, string ownershipPercentageFromForm)
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
                ownershipPercentage = GetOwnershipPercentage(uncreditedPercentage, form["ownershipPercentageSeller"][seller]);
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
                ownershipPercentage = GetOwnershipPercentage(uncreditedClickedBuyer, form["ownershipPercentageBuyer"][buyer]);
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
            if (CheckYearAlreadyExists(currentForm, _context) && currentForm.NatureOfTheDeed == standardPatrimonyRegularisation)
            {
                List<MultiOwner> higherInscriptionNumberMultiOwners = GetMultiOwnersWithHigherInscriptionNumber(_context, currentForm);
                List<MultiOwner> previousMultiOwners = GetMultiOwnersWithLowerInscriptionNumber(_context, currentForm);
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
            List<Person> buyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false).ToList();
            List<Person> sellers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true).ToList();
            List<Person> uncreditedOwnershipBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false && s.UncreditedOwnership == true).ToList();
            List<Person> creditedOwnershipBuyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false && s.UncreditedOwnership == false).ToList();
            double? ownershipPercentageToAssign = GetAssignedOwnershipPercentage(creditedOwnershipBuyers, uncreditedOwnershipBuyers);
            foreach (var uncreditedOwnershipBuyer in uncreditedOwnershipBuyers)
            {
                uncreditedOwnershipBuyer.OwnershipPercentage = ownershipPercentageToAssign;

            }
            await AddNewMultiOwners(_context, buyers, currentForm);
            await SetFinalYearPreviousMultiOwners(_context, currentForm);
        }

        private static async Task ManageCompraventa(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            List<Person> buyers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == false).ToList();
            List<Person> sellers = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true).ToList();
            double? totalBuyersSum = buyers.Sum(item => item.OwnershipPercentage);
            List<string> ruts = sellers.Select(o => o.Rut).ToList();
            List<MultiOwner> allOwnersForPeriod = GetAllOwnersFromPeriod(_context);
            List<MultiOwner> sellerMultiOwners = GetSellerOwnersFromPeriod(_context, ruts);
            if (totalBuyersSum == 100)
            {
                AssignCompraventaOwnershipPercentage(buyers, sellerMultiOwners);
                await AddNewMultiOwners(_context, buyers, currentForm);
                List<MultiOwner> multiownersToDelete = new List<MultiOwner>();
                SetMultiOwnersToDelete(ref multiownersToDelete, sellerMultiOwners, adjustedYear);
                _context.RemoveRange(multiownersToDelete);
            }
            else if (totalBuyersSum < 100 && totalBuyersSum > 0 && buyers.Count() == 1 && sellers.Count() == 1)
            {
                AssignCompraventaOwnershipPercentage(buyers, sellerMultiOwners);
                double? updatedOwnershipSeller = sellerMultiOwners[0].OwnershipPercentage - sellerMultiOwners[0].OwnershipPercentage * sellers[0].OwnershipPercentage / 100;
                MultiOwner sellerMO = sellerMultiOwners[0];
                Person seller = sellers[0];
                SetMultiOwnerRecords(adjustedYear, ref sellerMO, updatedOwnershipSeller, ref seller, ref buyers);
                await AddNewMultiOwners(_context, buyers, currentForm);
            }
            else
            { 
                foreach (var rut in ruts)
                {
                    Person currentSeller = _context.People.Where(s => s.FormsId == currentForm.AttentionNumber && s.Seller == true && s.Rut == rut).
                                            OrderBy(tableKey => tableKey.Id).LastOrDefault();
                    MultiOwner currentMultiOwner = GetOwnerRecordByRut(_context, rut);
                    double? newOwnershipPercentage = currentMultiOwner.OwnershipPercentage - currentSeller.OwnershipPercentage;
                    SetMultiOwnerRecords(adjustedYear, ref currentMultiOwner, newOwnershipPercentage, ref currentSeller, ref buyers);
                }
                await AddNewMultiOwners(_context, buyers, currentForm);
            }
            OldRecordKeeping(_context, currentForm, allOwnersForPeriod, sellerMultiOwners, adjustedYear);
            MergeDuplicateRegisters(_context, currentForm, adjustedYear, ruts);
            await SetNegativePercentagesToZero(_context, currentForm);
            await AdjustPercentagesPastLimit(_context, currentForm);
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
                    MultiOwner newMultiOwner = new MultiOwner(buyer.Rut, buyer.OwnershipPercentage,
                                            currentForm.Commune, currentForm.Block, currentForm.Property,
                                            currentForm.Sheets, currentForm.InscriptionDate,
                                            currentForm.InscriptionNumber, adjustedYear, validityYearFinish);
                    _context.Add(newMultiOwner);

                }
                else
                {
                    int? validityYearFinish = null;
                    MultiOwner newMultiOwner = new MultiOwner(buyer.Rut, buyer.OwnershipPercentage, currentForm.Commune,
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

        private static RealStateForm GetLastFormsRecord(InscripcionesBrDbContext _context)
        {
            return _context.RealStateForms.OrderBy(tableKey => tableKey.AttentionNumber).LastOrDefault();
        }

        private static void AssignCompraventaOwnershipPercentage(List<Person> buyers, List<MultiOwner> sellers)
        {
            double? totalSellersSum = sellers?.Sum(m => m.OwnershipPercentage);
            foreach (var buyer in buyers)
            {
                buyer.OwnershipPercentage = totalSellersSum * (buyer.OwnershipPercentage / 100);

            }
        }

        private static void MergeDuplicateRegisters(InscripcionesBrDbContext _context, RealStateForm currentForm, int adjustedYear, List<string> ruts)
        {
            List<MultiOwner> allMultiOwners = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune)
                                                    .ToList();
            List<String> newRuts = new();
            foreach (var multiowner in allMultiOwners)
            {
                List<MultiOwner> multiownersWithSameRut = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.Rut == multiowner.Rut && item.ValidityYearBegin == multiowner.ValidityYearBegin)
                                                    .ToList();
                if (multiownersWithSameRut.Count() > 1 && !ruts.Contains(multiowner.Rut))
                {
                    newRuts.Add(multiowner.Rut);
                }
            }
            List<string> filteredRuts = newRuts
                            .Where((s, index) => index % 2 == 0) // Filter by index being even (pair)
                            .ToList();
            foreach (var rut in filteredRuts)
            {
                MultiOwner multiOwnerHighestNumber = _context.MultiOwners.Where(item => item.Rut == rut && item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune).
                                                OrderByDescending(item => item.InscriptionNumber).FirstOrDefault();
                List<MultiOwner> multiownersWithSameRut = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.Rut == rut).ToList();
                List<MultiOwner> multiownersToDelete = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.Rut == rut && item.InscriptionNumber != multiOwnerHighestNumber.InscriptionNumber).ToList();
                double? mergeOwnership = multiownersWithSameRut.Sum(item => item.OwnershipPercentage);
                MultiOwner multiOwnerToMerge = new MultiOwner(multiOwnerHighestNumber.Rut, mergeOwnership,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    multiOwnerHighestNumber.InscriptionNumber, adjustedYear, multiOwnerHighestNumber.ValidityYearFinish);
                _context.Add(multiOwnerToMerge);
                _context.RemoveRange(multiownersWithSameRut);
            }
        }

        private static void OldRecordKeeping(InscripcionesBrDbContext _context, RealStateForm currentForm, List<MultiOwner> allOwnersForPeriod, List<MultiOwner> sellerMultiOwners, int adjustedYear)
        {
            foreach (var owner in allOwnersForPeriod)
            {
                if (!sellerMultiOwners.Contains(owner) && owner.ValidityYearBegin != adjustedYear)
                {
                    MultiOwner previousMultiOwner = new MultiOwner(owner.Rut, owner.OwnershipPercentage,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    owner.InscriptionNumber, owner.ValidityYearBegin, adjustedYear - 1);
                    owner.ValidityYearBegin = adjustedYear;
                    _context.Add(previousMultiOwner);
                }
            }
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
                MultiOwner correctedMultiOwner = new MultiOwner(negativeOwner.Rut, 0,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    negativeOwner.InscriptionNumber, negativeOwner.ValidityYearBegin, negativeOwner.ValidityYearFinish);
                _context.Add(correctedMultiOwner);
            }
            _context.RemoveRange(negativeOwnershipMultiOwners);
            await _context.SaveChangesAsync();
        }


            private static async Task AdjustPercentagesPastLimit(InscripcionesBrDbContext _context, RealStateForm currentForm)
        {
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            List<MultiOwner> sameYearMultiOwners = _context.MultiOwners.Where(item => item.Property == currentForm.Property &&
                                                    item.Block == currentForm.Block && item.Commune == currentForm.Commune
                                                    && item.ValidityYearBegin <= adjustedYear &&
                                                    (item.ValidityYearFinish == null || item.ValidityYearFinish > adjustedYear))
                                                    .ToList();
            double? sumOfOwnerships = sameYearMultiOwners.Sum(item => item.OwnershipPercentage);
            if (sumOfOwnerships > 100)
            {
                double? ratio = 100 / sumOfOwnerships;
                foreach (var owner in sameYearMultiOwners)
                {
                    MultiOwner correctedMultiOwner = new MultiOwner(owner.Rut, owner.OwnershipPercentage * ratio,
                                    currentForm.Commune, currentForm.Block, currentForm.Property,
                                    currentForm.Sheets, currentForm.InscriptionDate,
                                    owner.InscriptionNumber, owner.ValidityYearBegin, owner.ValidityYearFinish);
                    _context.Add(correctedMultiOwner);
                }
                _context.RemoveRange(sameYearMultiOwners);
            }
            await _context.SaveChangesAsync();
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

        private static MultiOwner FindNextOwner(InscripcionesBrDbContext _context, RealStateForm currentForm)
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

        private static MultiOwner GetOwnerRecordByRut(InscripcionesBrDbContext _context, string rut)
        {
            RealStateForm currentForm = GetLastFormsRecord(_context);
            return _context.MultiOwners.Where(m => m.Rut == rut && m.Property == currentForm.Property &&
                                                       m.Block == currentForm.Block && m.Commune == currentForm.Commune &&
                                                       m.ValidityYearFinish == null).
                                                       OrderBy(tableKey => tableKey.Id).LastOrDefault();
        }

        private static List<MultiOwner> GetAllOwnersFromPeriod(InscripcionesBrDbContext _context)
        {
            RealStateForm currentForm = GetLastFormsRecord(_context);
            int adjustedYear = AdjustYear(currentForm.InscriptionDate.Year);
            return _context.MultiOwners.Where(m => m.Property == currentForm.Property &&
                                               m.Block == currentForm.Block && m.Commune == currentForm.Commune &&
                                               m.ValidityYearBegin <= adjustedYear && (m.ValidityYearFinish > adjustedYear ||
                                               m.ValidityYearFinish == null)).ToList(); ;
        }

        private static List<MultiOwner> GetSellerOwnersFromPeriod(InscripcionesBrDbContext _context, List<String> ruts)
        {
            List<MultiOwner> sellerMultiOwners = new List<MultiOwner>();
            foreach (var rut in ruts)
            {
                MultiOwner sellerMultiOwner = GetOwnerRecordByRut(_context, rut);
                sellerMultiOwners.Add(sellerMultiOwner);
            }
            return sellerMultiOwners;
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

