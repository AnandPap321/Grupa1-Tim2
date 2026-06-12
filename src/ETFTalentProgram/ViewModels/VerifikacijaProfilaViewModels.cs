using ETFTalentProgram.Models;

namespace ETFTalentProgram.ViewModels
{
    public class VerifikacijaProfilaIndexViewModel
    {
        public List<StudentProfilVerifikacijaViewModel> StudentProfili { get; set; } = [];
        public List<FirmaProfilVerifikacijaViewModel> FirmaProfili { get; set; } = [];
    }

    public class StudentProfilVerifikacijaViewModel
    {
        public long ProfilId { get; set; }
        public long StudentId { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BrojIndeksa { get; set; } = string.Empty;
        public double Rang { get; set; }
        public StatusVerifikacije StatusVerifikacije { get; set; }
        public DateTime DatumAzuriranja { get; set; }
    }

    public class FirmaProfilVerifikacijaViewModel
    {
        public long ProfilId { get; set; }
        public long FirmaId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Lokacija { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public StatusVerifikacije StatusVerifikacije { get; set; }
        public DateTime DatumAzuriranja { get; set; }
    }
}
