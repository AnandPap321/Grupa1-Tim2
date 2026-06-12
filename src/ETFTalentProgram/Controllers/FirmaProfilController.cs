using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = AppRoles.Firma)]
    public class FirmaProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public FirmaProfilController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
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
                await _logService.InfoAsync("FIRMA_PROFIL_KREIRAN", $"Kreiran profil firme ID {firmaProfil.FirmaId}.");
                return RedirectToAction(nameof(Index));
            }
            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // GET: FirmaProfil/Edit - Gets current user's profile
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var firma = await _context.Firme.FirstOrDefaultAsync(f => f.Email == User.Identity.Name);
            if (firma == null)
            {
                return NotFound();
            }

            var firmaProfil = await _context.FirmaProfili.FirstOrDefaultAsync(f => f.FirmaId == firma.Id);
            if (firmaProfil == null)
            {
                return NotFound();
            }

            ViewData["FirmaId"] = new SelectList(_context.Firme, "Id", "Id", firmaProfil.FirmaId);
            return View(firmaProfil);
        }

        // POST: FirmaProfil/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,KratakOpis,PunOpis,Lokacija,Website,KontaktEmail,Logotip,TehnologijeStack,DatumAzuriranja,FirmaId")] FirmaProfil firmaProfil)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firmaProfil);
                    await _context.SaveChangesAsync();
                    await _logService.InfoAsync("FIRMA_PROFIL_AZURIRAN", $"Azuriran profil firme ID {firmaProfil.FirmaId}.");
                    TempData["StatusMessage"] = "Profil je uspješno ažuriran.";
                    return RedirectToAction(nameof(Edit));
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
                await _logService.WarningAsync("FIRMA_PROFIL_OBRISAN", $"Obrisan profil firme ID {firmaProfil.FirmaId}.");
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
