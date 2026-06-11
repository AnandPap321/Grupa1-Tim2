using Microsoft.AspNetCore.Identity;

namespace ETFTalentProgram.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime DatumRegistracije { get; set; } = DateTime.UtcNow;
        public DateTime? DatumZadnjePrijave { get; set; }
    }
}
