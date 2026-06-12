using ETFTalentProgram.Data;
using ETFTalentProgram.Models;

namespace ETFTalentProgram.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task InfoAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.INFO);
        }

        public Task WarningAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.WARNING);
        }

        public Task ErrorAsync(string tipAkcije, string detalji)
        {
            return WriteAsync(tipAkcije, detalji, NivoLoga.ERROR);
        }

        private async Task WriteAsync(string tipAkcije, string detalji, NivoLoga nivo)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var email = httpContext?.User?.Identity?.Name;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString() ?? "Nepoznata IP adresa";
            var userDetails = string.IsNullOrWhiteSpace(email)
                ? "Anonimni korisnik"
                : $"Korisnik: {email}";

            _context.Logovi.Add(new Log
            {
                TipAkcije = tipAkcije,
                VrijemeAkcije = DateTime.UtcNow,
                KorisnikId = null,
                IpAdresa = ipAddress,
                Detalji = $"{userDetails}. {detalji}",
                Nivo = nivo
            });

            await _context.SaveChangesAsync();
        }
    }
}
