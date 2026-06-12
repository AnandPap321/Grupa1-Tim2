using System.ComponentModel.DataAnnotations;
using ETFTalentProgram.Constants;

namespace ETFTalentProgram.ViewModels
{
    public class UserManagementIndexViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public DateTime DatumRegistracije { get; set; }
        public DateTime? DatumZadnjePrijave { get; set; }
    }

    public class UserManagementCreateViewModel
    {
        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lozinka je obavezna.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 znakova.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uloga je obavezna.")]
        public string Role { get; set; } = AppRoles.Student;
    }

    public class UserManagementEditViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email je obavezan.")]
        [EmailAddress(ErrorMessage = "Unesite ispravnu email adresu.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Uloga je obavezna.")]
        public string Role { get; set; } = AppRoles.Student;

        public bool EmailConfirmed { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Lozinka mora imati najmanje 6 znakova.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }
    }
}
