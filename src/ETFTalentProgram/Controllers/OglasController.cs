using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ETFTalentProgram.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;

namespace ETFTalentProgram.Controllers
{
    public class OglasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public OglasController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        // GET: Oglas
        public async Task<IActionResult> Index()
        {
            var oglasiQuery = _context.Oglasi
                .Include(o => o.Firma)
                .OrderByDescending(o => o.StatusOglasa == StatusOglasa.AKTIVAN)
                .ThenBy(o => o.RokPrijave)
                .AsQueryable();

            var oglasi = await oglasiQuery.ToListAsync();

            if (User.IsInRole(AppRoles.Student))
            {
                var student = await GetOrCreateCurrentStudentAsync();
                ViewData["AppliedOglasIds"] = await _context.PrijaveOglasa
                    .Where(p => p.StudentId == student.Id)
                    .Select(p => p.OglasId)
                    .ToHashSetAsync();
            }

            return View(oglasi);
        }

        // POST: Oglas/PrijaviSe/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> PrijaviSe(long id, string? propratniTekst)
        {
            var student = await GetOrCreateCurrentStudentAsync();
            var oglas = await _context.Oglasi.FirstOrDefaultAsync(o => o.Id == id);

            if (oglas == null)
            {
                return NotFound();
            }

            if (oglas.StatusOglasa != StatusOglasa.AKTIVAN)
            {
                TempData["StatusMessage"] = "Na ovaj oglas se trenutno nije moguce prijaviti.";
                return RedirectToAction(nameof(Index));
            }

            var alreadyApplied = await _context.PrijaveOglasa
                .AnyAsync(p => p.StudentId == student.Id && p.OglasId == id);

            if (alreadyApplied)
            {
                TempData["StatusMessage"] = "Vec ste poslali prijavu na ovaj oglas.";
                return RedirectToAction(nameof(Index));
            }

            var prijava = new PrijavaOglas
            {
                DatumPrijave = DateTime.UtcNow,
                PropratniTekst = string.IsNullOrWhiteSpace(propratniTekst)
                    ? "Prijava poslana putem ETF Talent Programa."
                    : propratniTekst.Trim(),
                StatusPrijave = StatusPrijave.NOVA,
                StudentId = student.Id,
                OglasId = oglas.Id
            };

            _context.PrijaveOglasa.Add(prijava);
            await _context.SaveChangesAsync();
            await _logService.InfoAsync("PRIJAVA_KREIRANA", $"Student ID {student.Id} se prijavio na oglas ID {oglas.Id}.");

            TempData["StatusMessage"] = "Prijava je uspjesno poslana.";
            return RedirectToAction("MojePrijave", "Student");
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

            if (User.IsInRole(AppRoles.Student))
            {
                var student = await GetOrCreateCurrentStudentAsync();
                ViewData["IsApplied"] = await _context.PrijaveOglasa
                    .AnyAsync(p => p.StudentId == student.Id && p.OglasId == oglas.Id);
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
                await _logService.InfoAsync("OGLAS_KREIRAN", $"Kreiran oglas ID {oglas.Id}: {oglas.Naslov}.");
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
                    await _logService.InfoAsync("OGLAS_AZURIRAN", $"Azuriran oglas ID {oglas.Id}: {oglas.Naslov}.");
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
                await _logService.WarningAsync("OGLAS_OBRISAN", $"Obrisan oglas ID {oglas.Id}: {oglas.Naslov}.");
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OglasExists(long id)
        {
            return _context.Oglasi.Any(e => e.Id == id);
        }

        private async Task<Student> GetOrCreateCurrentStudentAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == email);

            if (student != null)
            {
                return student;
            }

            student = new Student
            {
                Ime = GetNameFromEmail(email),
                Prezime = string.Empty,
                BrIndeksa = string.Empty,
                GodinaStudija = 0,
                GodinaUpisa = DateTime.Today.Year,
                ProsjekOcjena = 0,
                Verificiran = false,
                Email = email,
                Lozinka = string.Empty,
                Uloga = Uloga.STUDENT,
                Status = Status.AKTIVAN,
                DatumRegistracije = DateTime.UtcNow,
                DatumZadnjePrijave = DateTime.UtcNow
            };

            _context.Studenti.Add(student);
            await _context.SaveChangesAsync();

            return student;
        }

        private static string GetNameFromEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email[..atIndex] : email;
        }
    }
}
