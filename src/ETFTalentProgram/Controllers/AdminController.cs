using ETFTalentProgram.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ETFTalentProgram.Controllers
{
    [Authorize(Roles = AppRoles.Administrator)]
    public class AdminController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
