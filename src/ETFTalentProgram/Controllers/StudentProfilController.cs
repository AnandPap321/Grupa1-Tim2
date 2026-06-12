using ETFTalentProgram.Constants;
using ETFTalentProgram.Data;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using ETFTalentProgram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = $"{AppRoles.Student},{AppRoles.Firma},{AppRoles.Referent}")]
    public class StudentProfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;

        public StudentProfilController(ApplicationDbContext context, ILogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> Index()
        {
            var profil = await GetOrCreateCurrentStudentProfileAsync();
            var model = await BuildViewModelAsync(profil, isReadOnly: false, canSendOffer: false);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Student)]
        public async Task<IActionResult> Update(StudentProfilViewModel model)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (profil == null || !IsCurrentStudent(profil.Student))
            {
                return Forbid();
            }

            profil.Biografija = NormalizeText(model.Biografija);
            profil.Vjestine = NormalizeCommaList(model.Vjestine);
            profil.PreferiraneTehnologije = profil.Vjestine;
            profil.Projekti = NormalizeProjects(model.Projekti);
            profil.PreferiraneLokacije = NormalizeCommaList(model.PreferiraneLokacije);
            profil.DostupanOd = model.DostupanOd == default ? DateTime.Today : model.DostupanOd;
            profil.Rang = CalculateRank(profil.Student, profil.Vjestine, profil.Projekti);
            profil.DatumAzuriranja = DateTime.UtcNow;
            profil.StatusVerifikacije = profil.Student.Verificiran
                ? StatusVerifikacije.VERIFICIRAN
                : StatusVerifikacije.NA_CEKANJU;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_PROFIL_AZURIRAN", $"Azuriran profil studenta ID {profil.StudentId}.");
            TempData["StatusMessage"] = "Profil je uspješno sačuvan.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(long id)
        {
            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            var isOwner = User.IsInRole(AppRoles.Student) && IsCurrentStudent(profil.Student);
            var canView = isOwner || User.IsInRole(AppRoles.Firma) || User.IsInRole(AppRoles.Referent);
            if (!canView)
            {
                return Forbid();
            }

            var model = await BuildViewModelAsync(
                profil,
                isReadOnly: true,
                canSendOffer: User.IsInRole(AppRoles.Firma));

            return View(model);
        }

        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (profil == null)
            {
                return NotFound();
            }

            ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Email", profil.StudentId);
            return View(profil);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppRoles.Referent)]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Rang,Biografija,Vjestine,Projekti,PreferiraneLokacije,PreferiraneTehnologije,DostupanOd,DatumAzuriranja,StatusVerifikacije,StudentId")] StudentProfil profil)
        {
            if (id != profil.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewData["StudentId"] = new SelectList(_context.Studenti, "Id", "Email", profil.StudentId);
                return View(profil);
            }

            var existingProfil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProfil == null)
            {
                return NotFound();
            }

            existingProfil.Rang = profil.Rang;
            existingProfil.Biografija = NormalizeText(profil.Biografija);
            existingProfil.Vjestine = NormalizeCommaList(profil.Vjestine);
            existingProfil.Projekti = NormalizeProjects(profil.Projekti);
            existingProfil.PreferiraneLokacije = NormalizeCommaList(profil.PreferiraneLokacije);
            existingProfil.PreferiraneTehnologije = NormalizeCommaList(profil.PreferiraneTehnologije);
            existingProfil.DostupanOd = profil.DostupanOd == default ? DateTime.Today : profil.DostupanOd;
            existingProfil.DatumAzuriranja = DateTime.UtcNow;
            existingProfil.StatusVerifikacije = profil.StatusVerifikacije;
            existingProfil.Student.Verificiran = profil.StatusVerifikacije == StatusVerifikacije.VERIFICIRAN;

            await _context.SaveChangesAsync();
            await _logService.InfoAsync("STUDENT_PROFIL_REFERENT_AZURIRAN", $"Referent je azurirao profil studenta ID {existingProfil.StudentId} sa statusom {existingProfil.StatusVerifikacije}.");
            TempData["StatusMessage"] = "Studentski profil je azuriran.";

            return RedirectToAction("Index", "Verifikacija");
        }

        [Authorize(Roles = AppRoles.Firma)]
        public async Task<IActionResult> Search(string? q, double? minimalniRang, string? tehnologija)
        {
            var profili = await _context.StudentProfili
                .Include(p => p.Student)
                .ToListAsync();

            var normalizedQuery = NormalizeText(q).ToLowerInvariant();
            var normalizedTechnology = NormalizeText(tehnologija).ToLowerInvariant();

            var filtered = profili
                .Where(p => string.IsNullOrEmpty(normalizedQuery)
                    || (p.Student != null && $"{p.Student.Ime} {p.Student.Prezime}".ToLowerInvariant().Contains(normalizedQuery))
                    || (p.Vjestine ?? string.Empty).ToLowerInvariant().Contains(normalizedQuery))
                .Where(p => minimalniRang == null || p.Rang >= minimalniRang)
                .Where(p => string.IsNullOrEmpty(normalizedTechnology)
                    || (p.Vjestine ?? string.Empty).ToLowerInvariant().Contains(normalizedTechnology)
                    || (p.PreferiraneTehnologije ?? string.Empty).ToLowerInvariant().Contains(normalizedTechnology))
                .OrderByDescending(p => p.Rang)
                .ThenBy(p => p.Student != null ? p.Student.Prezime : string.Empty)
                .ToList();

            var models = new List<StudentProfilViewModel>();
            foreach (var profil in filtered)
            {
                models.Add(await BuildViewModelAsync(profil, isReadOnly: true, canSendOffer: true));
            }

            ViewData["Query"] = q;
            ViewData["MinimalniRang"] = minimalniRang;
            ViewData["Tehnologija"] = tehnologija;

            return View(models);
        }

        private async Task<StudentProfil> GetOrCreateCurrentStudentProfileAsync()
        {
            var email = User.Identity?.Name ?? string.Empty;
            var student = await _context.Studenti.FirstOrDefaultAsync(s => s.Email == email);

            if (student == null)
            {
                student = new Student
                {
                    Ime = email.Contains('@') ? email[..email.IndexOf('@')] : "Student",
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
            }

            var profil = await _context.StudentProfili
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.StudentId == student.Id);

            if (profil != null)
            {
                return profil;
            }

            profil = new StudentProfil
            {
                StudentId = student.Id,
                Student = student,
                Rang = CalculateRank(student, string.Empty, string.Empty),
                Biografija = string.Empty,
                Vjestine = string.Empty,
                PreferiraneTehnologije = string.Empty,
                Projekti = string.Empty,
                PreferiraneLokacije = string.Empty,
                DostupanOd = DateTime.Today,
                DatumAzuriranja = DateTime.UtcNow,
                StatusVerifikacije = student.Verificiran
                    ? StatusVerifikacije.VERIFICIRAN
                    : StatusVerifikacije.NA_CEKANJU
            };

            _context.StudentProfili.Add(profil);
            await _context.SaveChangesAsync();

            return profil;
        }

        private async Task<StudentProfilViewModel> BuildViewModelAsync(StudentProfil profil, bool isReadOnly, bool canSendOffer)
        {
            if (profil.Student == null)
            {
                await _context.Entry(profil).Reference(p => p.Student).LoadAsync();
            }

            var ects = await _context.AkademskiPodaci
                .Where(a => a.StudentId == profil.StudentId)
                .SumAsync(a => (int?)a.ECTS) ?? 0;

            return StudentProfilViewModel.From(profil, ects, isReadOnly, canSendOffer);
        }

        private bool IsCurrentStudent(Student? student)
        {
            return student != null
                && string.Equals(student.Email, User.Identity?.Name, StringComparison.OrdinalIgnoreCase);
        }

        private static double CalculateRank(Student student, string? vjestine, string? projekti)
        {
            var skillCount = CountCommaItems(vjestine);
            var projectCount = CountProjectItems(projekti);
            var academicScore = Math.Clamp(student.ProsjekOcjena, 0, 10) * 0.6;
            var skillScore = Math.Min(skillCount, 10) / 10.0 * 2;
            var projectScore = Math.Min(projectCount, 5) / 5.0 * 2;

            return Math.Round(academicScore + skillScore + projectScore, 1);
        }

        private static int CountCommaItems(string? value)
        {
            return NormalizeCommaList(value)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }

        private static int CountProjectItems(string? value)
        {
            return NormalizeProjects(value)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Length;
        }

        private static string NormalizeText(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string NormalizeCommaList(string? value)
        {
            return string.Join(", ", (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static string NormalizeProjects(string? value)
        {
            return string.Join(Environment.NewLine, (value ?? string.Empty)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
