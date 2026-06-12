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
                return "/ReferentKorisnici";

            if (roles.Contains(AppRoles.Firma))
                return "/FirmaProfil";

            if (roles.Contains(AppRoles.Student))
                return "/Student";

            return null;
        }
    }
}
