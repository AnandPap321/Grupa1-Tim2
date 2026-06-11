using ETFTalentProgram.Models;

namespace ETFTalentProgram.ViewModels
{
    public class StudentPocetnaViewModel
    {
        public Student Student { get; set; } = new();
        public StudentProfil? Profil { get; set; }
        public IReadOnlyList<Oglas> NajnovijiOglasi { get; set; } = [];
        public int BrojPrijava { get; set; }
        public int BrojPonuda { get; set; }
        public int BrojProjekata { get; set; }

        public double Rang => Profil?.Rang ?? 0;
        public StatusVerifikacije StatusVerifikacije => Profil?.StatusVerifikacije ?? StatusVerifikacije.NA_CEKANJU;
        public string PunoIme => string.Join(" ", new[] { Student.Ime, Student.Prezime }.Where(x => !string.IsNullOrWhiteSpace(x)));
        public string Inicijali => string.Concat(new[] { Student.Ime, Student.Prezime }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => char.ToUpperInvariant(x.Trim()[0]))).PadRight(2, 'S')[..2];
    }
}
