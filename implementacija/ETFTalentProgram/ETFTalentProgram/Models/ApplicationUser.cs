using Microsoft.AspNetCore.Identity;

namespace ETFTalentProgram.Models
{
    public class ApplicationUser : IdentityUser
    {
        public Uloga Uloga { get; set; }
        public Status Status { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public DateTime DatumZadnjePrijave { get; set; }

        public ApplicationUser() { }
    }
}