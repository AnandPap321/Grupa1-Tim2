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
    public class VerifikacijaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VerifikacijaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Verifikacija
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Verifikacije.Include(v => v.Referent).Include(v => v.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Verifikacija/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije
                .Include(v => v.Referent)
                .Include(v => v.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (verifikacija == null)
            {
                return NotFound();
            }

            return View(verifikacija);
        }

        // GET: Verifikacija/Create
        public IActionResult Create()
        {
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id");
            return View();
        }

        // POST: Verifikacija/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DatumPodnosenja,DatumVerifikacije,StatusVerifikacije,Komentar,Dokumenti,StudentId,ReferentId")] Verifikacija verifikacija)
        {
            if (ModelState.IsValid)
            {
                _context.Add(verifikacija);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // GET: Verifikacija/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije.FindAsync(id);
            if (verifikacija == null)
            {
                return NotFound();
            }
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // POST: Verifikacija/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DatumPodnosenja,DatumVerifikacije,StatusVerifikacije,Komentar,Dokumenti,StudentId,ReferentId")] Verifikacija verifikacija)
        {
            if (id != verifikacija.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(verifikacija);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VerifikacijaExists(verifikacija.Id))
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
            ViewData["ReferentId"] = new SelectList(_context.Referenti, "Id", "Id", verifikacija.ReferentId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", verifikacija.StudentId);
            return View(verifikacija);
        }

        // GET: Verifikacija/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verifikacija = await _context.Verifikacije
                .Include(v => v.Referent)
                .Include(v => v.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (verifikacija == null)
            {
                return NotFound();
            }

            return View(verifikacija);
        }

        // POST: Verifikacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var verifikacija = await _context.Verifikacije.FindAsync(id);
            if (verifikacija != null)
            {
                _context.Verifikacije.Remove(verifikacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VerifikacijaExists(long id)
        {
            return _context.Verifikacije.Any(e => e.Id == id);
        }
    }
}
