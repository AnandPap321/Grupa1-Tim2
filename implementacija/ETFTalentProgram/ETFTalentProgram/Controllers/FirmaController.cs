using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;

namespace ETFTalentProgram.Controllers
{
    public class FirmaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FirmaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Firma
        public async Task<IActionResult> Index()
        {
            return View(await _context.Firme.ToListAsync());
        }

        // GET: Firma/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // GET: Firma/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Firma/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Naziv,OpisFirme,Lokacija,Website,KontaktEmail,IndustrijskiSektor,VelicinaFirme,GodinaOsnivanja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Firma firma)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firma);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(firma);
        }

        // GET: Firma/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme.FindAsync(id);
            if (firma == null)
            {
                return NotFound();
            }
            return View(firma);
        }

        // POST: Firma/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Naziv,OpisFirme,Lokacija,Website,KontaktEmail,IndustrijskiSektor,VelicinaFirme,GodinaOsnivanja,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Firma firma)
        {
            if (id != firma.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firma);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmaExists(firma.Id))
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
            return View(firma);
        }

        // GET: Firma/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firma = await _context.Firme
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firma == null)
            {
                return NotFound();
            }

            return View(firma);
        }

        // POST: Firma/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var firma = await _context.Firme.FindAsync(id);
            if (firma != null)
            {
                _context.Firme.Remove(firma);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmaExists(long id)
        {
            return _context.Firme.Any(e => e.Id == id);
        }
    }
}
