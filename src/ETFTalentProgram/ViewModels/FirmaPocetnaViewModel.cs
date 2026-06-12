using ETFTalentProgram.Models;

namespace ETFTalentProgram.ViewModels
{
    public class FirmaPocetnaViewModel
    {
        public Firma Firma { get; set; } = new();
        public FirmaProfil? Profil { get; set; }
        public IReadOnlyList<Oglas> NajnovijiOglasi { get; set; } = [];
        public int BrojOglasa { get; set; }
        public int BrojAktivnihOglasa { get; set; }
        public int BrojPrijava { get; set; }
        public StatusVerifikacije StatusVerifikacije => Profil?.StatusVerifikacije ?? StatusVerifikacije.NA_CEKANJU;
    }
}
