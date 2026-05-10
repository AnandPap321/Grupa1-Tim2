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
    public class ReferentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReferentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Referent
        public async Task<IActionResult> Index()
        {
            return View(await _context.Referenti.ToListAsync());
        }

        // GET: Referent/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var referent = await _context.Referenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (referent == null)
            {
                return NotFound();
            }

            return View(referent);
        }

        // GET: Referent/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Referent/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ime,Prezime,Odsjek,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Referent referent)
        {
            if (ModelState.IsValid)
            {
                _context.Add(referent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(referent);
        }

        // GET: Referent/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var referent = await _context.Referenti.FindAsync(id);
            if (referent == null)
            {
                return NotFound();
            }
            return View(referent);
        }

        // POST: Referent/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Ime,Prezime,Odsjek,Id,Email,Lozinka,Uloga,Status,DatumRegistracije,DatumZadnjePrijave")] Referent referent)
        {
            if (id != referent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(referent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReferentExists(referent.Id))
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
            return View(referent);
        }

        // GET: Referent/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var referent = await _context.Referenti
                .FirstOrDefaultAsync(m => m.Id == id);
            if (referent == null)
            {
                return NotFound();
            }

            return View(referent);
        }

        // POST: Referent/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var referent = await _context.Referenti.FindAsync(id);
            if (referent != null)
            {
                _context.Referenti.Remove(referent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReferentExists(long id)
        {
            return _context.Referenti.Any(e => e.Id == id);
        }
    }
}
