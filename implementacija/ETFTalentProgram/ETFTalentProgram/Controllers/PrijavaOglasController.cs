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
    public class PrijavaOglasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrijavaOglasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PrijavaOglas
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PrijaveOglasa.Include(p => p.Oglas).Include(p => p.Student);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PrijavaOglas/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijavaOglas = await _context.PrijaveOglasa
                .Include(p => p.Oglas)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijavaOglas == null)
            {
                return NotFound();
            }

            return View(prijavaOglas);
        }

        // GET: PrijavaOglas/Create
        public IActionResult Create()
        {
            ViewData["OglasId"] = new SelectList(_context.Oglasi, "Id", "Id");
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id");
            return View();
        }

        // POST: PrijavaOglas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DatumPrijave,PropratniTekst,StatusPrijave,DatumOdgovora,StudentId,OglasId")] PrijavaOglas prijavaOglas)
        {
            if (ModelState.IsValid)
            {
                _context.Add(prijavaOglas);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["OglasId"] = new SelectList(_context.Oglasi, "Id", "Id", prijavaOglas.OglasId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", prijavaOglas.StudentId);
            return View(prijavaOglas);
        }

        // GET: PrijavaOglas/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijavaOglas = await _context.PrijaveOglasa.FindAsync(id);
            if (prijavaOglas == null)
            {
                return NotFound();
            }
            ViewData["OglasId"] = new SelectList(_context.Oglasi, "Id", "Id", prijavaOglas.OglasId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", prijavaOglas.StudentId);
            return View(prijavaOglas);
        }

        // POST: PrijavaOglas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,DatumPrijave,PropratniTekst,StatusPrijave,DatumOdgovora,StudentId,OglasId")] PrijavaOglas prijavaOglas)
        {
            if (id != prijavaOglas.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(prijavaOglas);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrijavaOglasExists(prijavaOglas.Id))
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
            ViewData["OglasId"] = new SelectList(_context.Oglasi, "Id", "Id", prijavaOglas.OglasId);
            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Id", prijavaOglas.StudentId);
            return View(prijavaOglas);
        }

        // GET: PrijavaOglas/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prijavaOglas = await _context.PrijaveOglasa
                .Include(p => p.Oglas)
                .Include(p => p.Student)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (prijavaOglas == null)
            {
                return NotFound();
            }

            return View(prijavaOglas);
        }

        // POST: PrijavaOglas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var prijavaOglas = await _context.PrijaveOglasa.FindAsync(id);
            if (prijavaOglas != null)
            {
                _context.PrijaveOglasa.Remove(prijavaOglas);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrijavaOglasExists(long id)
        {
            return _context.PrijaveOglasa.Any(e => e.Id == id);
        }
    }
}
