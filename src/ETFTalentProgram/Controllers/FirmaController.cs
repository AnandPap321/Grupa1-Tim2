using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.ViewModels;

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
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Index()
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var profil = await GetOrCreateCurrentFirmaProfileAsync(firma);
            var najnovijiOglasi = await _context.Oglasi
                .Where(o => o.FirmaId == firma.Id)
                .OrderByDescending(o => o.DatumObjave)
                .Take(5)
                .ToListAsync();

            var model = new FirmaPocetnaViewModel
            {
                Firma = firma,
                Profil = profil,
                NajnovijiOglasi = najnovijiOglasi,
                BrojOglasa = await _context.Oglasi.CountAsync(o => o.FirmaId == firma.Id),
                BrojAktivnihOglasa = await _context.Oglasi.CountAsync(o => o.FirmaId == firma.Id && o.StatusOglasa == StatusOglasa.AKTIVAN),
                BrojPrijava = await _context.PrijaveOglasa.CountAsync(p => p.Oglas.FirmaId == firma.Id)
            };

            return View(model);
        }

        // GET: Firma/Details/5
        [Authorize(Roles = AppRoles.Referent)]
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
        [Authorize(Roles = AppRoles.Referent)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Firma/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
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
        [Authorize(Roles = AppRoles.Referent)]
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
        [Authorize(Roles = AppRoles.Referent)]
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
        [Authorize(Roles = AppRoles.Referent)]
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
        [Authorize(Roles = AppRoles.Referent)]
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

        // GET: Firma/Profil_firme
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Profil_firme()
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            var firmaProfil = await GetOrCreateCurrentFirmaProfileAsync(firma);

            return View(firmaProfil);
        }

        // POST: Firma/Profil_firme
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Profil_firme([Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var firma = await GetOrCreateCurrentFirmaAsync();
                    var existingProfil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.Id == firmaProfil.Id && f.FirmaId == firma.Id);
                    if (existingProfil == null)
                    {
                        return Forbid();
                    }

                    existingProfil.KratakOpis = firmaProfil.KratakOpis;
                    existingProfil.PunOpis = firmaProfil.PunOpis;
                    existingProfil.Lokacija = firmaProfil.Lokacija;
                    existingProfil.Website = firmaProfil.Website;
                    existingProfil.KontaktEmail = firmaProfil.KontaktEmail;
                    existingProfil.Logotip = firmaProfil.Logotip;
                    existingProfil.TehnologijeStack = firmaProfil.TehnologijeStack;
                    existingProfil.DatumAzuriranja = DateTime.UtcNow;
                    existingProfil.StatusVerifikacije = StatusVerifikacije.NA_CEKANJU;
                    await _context.SaveChangesAsync();
                    TempData["StatusMessage"] = "Profil je uspješno ažuriran.";
                    return RedirectToAction(nameof(Profil_firme));
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
            }
            return View(firmaProfil);
        }

        private bool FirmaProfilExists(long id)
        {
            return _context.FirmaProfili.Any(e => e.Id == id);
        }

        // GET: Firma/Objavi_oglas
        [Authorize(Roles = AppRoles.Firma)]
        public IActionResult Objavi_oglas()
        {
            return View(new Oglas
            {
                DatumObjave = DateTime.Today,
                RokPrijave = DateTime.Today.AddDays(30),
                StatusOglasa = StatusOglasa.AKTIVAN
            });
        }

        // POST: Firma/Objavi_oglas
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Objavi_oglas([Bind("Id,Naslov,Opis,Tehnologije,RokPrijave,DatumObjave,TipOglasa,TipAngazmana,StatusOglasa,Lokacija,MinRang,MinProsjek,Kompenzacija,FirmaId")] Oglas oglas)
        {
            var firma = await GetOrCreateCurrentFirmaAsync();
            oglas.FirmaId = firma.Id;
            oglas.Kompenzacija = oglas.Kompenzacija?.Trim() ?? string.Empty;
            ModelState.Remove(nameof(Oglas.Firma));

            if (ModelState.IsValid)
            {
                oglas.DatumObjave = oglas.DatumObjave == default ? DateTime.Today : oglas.DatumObjave;
                oglas.RokPrijave = oglas.RokPrijave == default ? DateTime.Today.AddDays(30) : oglas.RokPrijave;
                _context.Add(oglas);
                await _context.SaveChangesAsync();
                TempData["StatusMessage"] = "Oglas je uspješno objavljen.";
                return RedirectToAction(nameof(Index));
            }
            return View(oglas);
        }

        private async Task<Firma> GetOrCreateCurrentFirmaAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var firma = await _context.Firme.FirstOrDefaultAsync(f => f.Email == email);

            if (firma != null)
            {
                return firma;
            }

            firma = new Firma
            {
                Naziv = GetNameFromEmail(email),
                OpisFirme = string.Empty,
                Lokacija = string.Empty,
                Website = string.Empty,
                KontaktEmail = email,
                IndustrijskiSektor = string.Empty,
                VelicinaFirme = VelicinaFirme.MALA,
                GodinaOsnivanja = DateTime.Today.Year,
                Email = email,
                Lozinka = string.Empty,
                Uloga = Uloga.FIRMA,
                Status = Status.AKTIVAN,
                DatumRegistracije = DateTime.UtcNow,
                DatumZadnjePrijave = DateTime.UtcNow
            };

            _context.Firme.Add(firma);
            await _context.SaveChangesAsync();

            return firma;
        }

        private async Task<FirmaProfil> GetOrCreateCurrentFirmaProfileAsync(Firma firma)
        {
            var profil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.FirmaId == firma.Id);
            if (profil != null)
            {
                return profil;
            }

            profil = new FirmaProfil
            {
                FirmaId = firma.Id,
                KratakOpis = string.Empty,
                PunOpis = string.Empty,
                Lokacija = firma.Lokacija,
                Website = firma.Website,
                KontaktEmail = firma.KontaktEmail,
                Logotip = string.Empty,
                TehnologijeStack = string.Empty,
                DatumAzuriranja = DateTime.UtcNow,
                StatusVerifikacije = StatusVerifikacije.NA_CEKANJU
            };

            _context.FirmaProfili.Add(profil);
            await _context.SaveChangesAsync();

            return profil;
        }

        private static string GetNameFromEmail(string email)
        {
            var atIndex = email.IndexOf('@');
            return atIndex > 0 ? email[..atIndex] : email;
        }
    }
}
