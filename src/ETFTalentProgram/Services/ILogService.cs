using ETFTalentProgram.Models;

namespace ETFTalentProgram.Services
{
    public interface ILogService
    {
        Task InfoAsync(string tipAkcije, string detalji);
        Task WarningAsync(string tipAkcije, string detalji);
        Task ErrorAsync(string tipAkcije, string detalji);
    }
}
