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
    public class FirmaProfilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FirmaProfilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FirmaProfil
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FirmaProfili.Include(f => f.Firma);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FirmaProfil/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili
                .Include(f => f.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmaProfil == null)
            {
                return NotFound();
            }

            return View(firmaProfil);
        }

        // GET: FirmaProfil/Create
        public IActionResult Create()
        {
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id");
            return View();
        }

        // POST: FirmaProfil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firmaProfil);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // GET: FirmaProfil/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili.FindAsync(id);
            if (firmaProfil == null)
            {
                return NotFound();
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // POST: FirmaProfil/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil)
        {
            if (id != firmaProfil.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firmaProfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmaProfilExists(firmaProfil.Id))
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
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // GET: FirmaProfil/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili
                .Include(f => f.Firma)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmaProfil == null)
            {
                return NotFound();
            }

            return View(firmaProfil);
        }

        // POST: FirmaProfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var firmaProfil = await _context.FirmaProfili.FindAsync(id);
            if (firmaProfil != null)
            {
                _context.FirmaProfili.Remove(firmaProfil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmaProfilExists(long id)
        {
            return _context.FirmaProfili.Any(e => e.Id == id);
        }
    }
}
