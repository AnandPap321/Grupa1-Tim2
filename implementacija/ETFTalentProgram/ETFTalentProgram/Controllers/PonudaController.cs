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
    public class PonudaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PonudaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ponuda
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ponude.Include(p => p.Posiljalac).Include(p => p.Primalac);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ponuda/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ponuda = await _context.Ponude
                .Include(p => p.Posiljalac)
                .Include(p => p.Primalac)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ponuda == null)
            {
                return NotFound();
            }

            return View(ponuda);
        }

        // GET: Ponuda/Create
        public IActionResult Create()
        {
            ViewData["PosiljalacId"] = new SelectList(_context.Korisnici, "Id", "Id");
            ViewData["PrimalacId"] = new SelectList(_context.Korisnici, "Id", "Id");
            return View();
        }

        // POST: Ponuda/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TekstPoruke,DatumSlanja,Status,TipPonude,DatumOdgovora,PosiljalacId,PrimalacId")] Ponuda ponuda)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ponuda);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PosiljalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PosiljalacId);
            ViewData["PrimalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PrimalacId);
            return View(ponuda);
        }

        // GET: Ponuda/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ponuda = await _context.Ponude.FindAsync(id);
            if (ponuda == null)
            {
                return NotFound();
            }
            ViewData["PosiljalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PosiljalacId);
            ViewData["PrimalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PrimalacId);
            return View(ponuda);
        }

        // POST: Ponuda/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,TekstPoruke,DatumSlanja,Status,TipPonude,DatumOdgovora,PosiljalacId,PrimalacId")] Ponuda ponuda)
        {
            if (id != ponuda.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ponuda);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PonudaExists(ponuda.Id))
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
            ViewData["PosiljalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PosiljalacId);
            ViewData["PrimalacId"] = new SelectList(_context.Korisnici, "Id", "Id", ponuda.PrimalacId);
            return View(ponuda);
        }

        // GET: Ponuda/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ponuda = await _context.Ponude
                .Include(p => p.Posiljalac)
                .Include(p => p.Primalac)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ponuda == null)
            {
                return NotFound();
            }

            return View(ponuda);
        }

        // POST: Ponuda/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var ponuda = await _context.Ponude.FindAsync(id);
            if (ponuda != null)
            {
                _context.Ponude.Remove(ponuda);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PonudaExists(long id)
        {
            return _context.Ponude.Any(e => e.Id == id);
        }
    }
}
