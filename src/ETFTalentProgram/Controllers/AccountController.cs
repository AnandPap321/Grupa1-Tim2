using ETFTalentProgram.Constants;
using ETFTalentProgram.Helpers;
using ETFTalentProgram.Models;
using ETFTalentProgram.Services;
using ETFTalentProgram.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ETFTalentProgram.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogService _logService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogService logService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logService = logService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var dashboardUrl = RoleRedirectHelper.GetDashboardUrl(roles);
                    if (dashboardUrl != null)
                        return Redirect(dashboardUrl);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                await _logService.WarningAsync("LOGIN_NEUSPJESAN", $"Pokusaj prijave za nepostojeci email: {model.Email}.");
                ModelState.AddModelError(string.Empty, "Neispravan email ili lozinka.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                await _logService.WarningAsync("LOGIN_NEUSPJESAN", $"Neuspjesna prijava za korisnika: {model.Email}.");
                ModelState.AddModelError(string.Empty, "Neispravan email ili lozinka.");
                return View(model);
            }

            user.DatumZadnjePrijave = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await _logService.InfoAsync("LOGIN_USPJESAN", $"Uspjesna prijava korisnika: {model.Email}.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var roles = await _userManager.GetRolesAsync(user);
            var dashboardUrl = RoleRedirectHelper.GetDashboardUrl(roles);
            if (dashboardUrl != null)
                return Redirect(dashboardUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterStudent()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await RegisterUserAsync(model, AppRoles.Student);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            await _logService.InfoAsync("REGISTRACIJA_STUDENT", $"Registrovan student: {model.Email}.");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterFirma()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterFirma(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await RegisterUserAsync(model, AppRoles.Firma);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            await _logService.InfoAsync("REGISTRACIJA_FIRMA", $"Registrovana firma: {model.Email}.");
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _logService.InfoAsync("LOGOUT", "Korisnik se odjavio iz sistema.");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<IdentityResult> RegisterUserAsync(RegisterViewModel model, string role)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                DatumRegistracije = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            return await _userManager.AddToRoleAsync(user, role);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
