using ETFTalentProgram.Constants;

namespace ETFTalentProgram.Helpers
{
    public static class RoleRedirectHelper
    {
        public static string? GetDashboardUrl(IList<string> roles)
        {
            if (roles.Contains(AppRoles.Administrator))
                return "/Admin/Dashboard";

            if (roles.Contains(AppRoles.Referent))
                return "/Verifikacija/Lista";

            if (roles.Contains(AppRoles.Firma))
                return "/FirmaProfil";

            if (roles.Contains(AppRoles.Student))
                return "/StudentProfil";

            return null;
        }
    }
}
