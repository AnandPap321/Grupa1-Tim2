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
    public class OglasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OglasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Oglas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Oglasi.Include(o => o.Firma);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Oglas/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oglas = await _context.Oglasi
                .Include(o => o.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (oglas == null)
            {
                return NotFound();
            }

            return View(oglas);
        }

        // GET: Oglas/Create
        public IActionResult Create()
        {
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id");
            return View();
        }

        // POST: Oglas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Naslov,Opis,Tehnologije,RokPrijave,DatumObjave,TipOglasa,TipAngazmana,StatusOglasa,Lokacija,MinRang,MinProsjek,Kompenzacija,FirmaId")] Oglas oglas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(oglas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", oglas.FirmaId);
            return View(oglas);
        }

        // GET: Oglas/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas == null)
            {
                return NotFound();
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", oglas.FirmaId);
            return View(oglas);
        }

        // POST: Oglas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Naslov,Opis,Tehnologije,RokPrijave,DatumObjave,TipOglasa,TipAngazmana,StatusOglasa,Lokacija,MinRang,MinProsjek,Kompenzacija,FirmaId")] Oglas oglas)
        {
            if (id != oglas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oglas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OglasExists(oglas.Id))
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
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", oglas.FirmaId);
            return View(oglas);
        }

        // GET: Oglas/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oglas = await _context.Oglasi
                .Include(o => o.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (oglas == null)
            {
                return NotFound();
            }

            return View(oglas);
        }

        // POST: Oglas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var oglas = await _context.Oglasi.FindAsync(id);
            if (oglas != null)
            {
                _context.Oglasi.Remove(oglas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OglasExists(long id)
        {
            return _context.Oglasi.Any(e => e.Id == id);
        }
    }
}
