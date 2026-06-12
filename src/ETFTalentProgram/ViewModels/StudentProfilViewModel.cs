using ETFTalentProgram.Models;

namespace ETFTalentProgram.ViewModels
{
    public class StudentProfilViewModel
    {
        public long Id { get; set; }
        public long StudentId { get; set; }
        public string Ime { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string BrojIndeksa { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int GodinaStudija { get; set; }
        public int GodinaUpisa { get; set; }
        public double ProsjekOcjena { get; set; }
        public int EctsBodovi { get; set; }
        public double Rang { get; set; }
        public string Biografija { get; set; } = string.Empty;
        public string Vjestine { get; set; } = string.Empty;
        public string Projekti { get; set; } = string.Empty;
        public string PreferiraneLokacije { get; set; } = string.Empty;
        public DateTime DostupanOd { get; set; }
        public DateTime DatumAzuriranja { get; set; }
        public StatusVerifikacije StatusVerifikacije { get; set; }
        public bool IsReadOnly { get; set; }
        public bool CanSendOffer { get; set; }

        public IReadOnlyList<string> VjestineLista => SplitTags(Vjestine);
        public IReadOnlyList<string> LokacijeLista => SplitTags(PreferiraneLokacije);
        public IReadOnlyList<StudentProjekatViewModel> ProjektiLista => SplitProjects(Projekti);
        public int BrojProjekata => ProjektiLista.Count;
        public int BrojVjestina => VjestineLista.Count;
        public string PunoIme => string.Join(" ", new[] { Ime, Prezime }.Where(x => !string.IsNullOrWhiteSpace(x)));
        public string Inicijali => string.Concat(new[] { Ime, Prezime }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => char.ToUpperInvariant(x.Trim()[0]))).PadRight(2, 'S')[..2];

        public static StudentProfilViewModel From(StudentProfil profil, int ectsBodovi, bool isReadOnly, bool canSendOffer)
        {
            var student = profil.Student;

            return new StudentProfilViewModel
            {
                Id = profil.Id,
                StudentId = profil.StudentId,
                Ime = student?.Ime ?? string.Empty,
                Prezime = student?.Prezime ?? string.Empty,
                BrojIndeksa = student?.BrIndeksa ?? string.Empty,
                Email = student?.Email ?? string.Empty,
                GodinaStudija = student?.GodinaStudija ?? 0,
                GodinaUpisa = student?.GodinaUpisa ?? DateTime.Today.Year,
                ProsjekOcjena = student?.ProsjekOcjena ?? 0,
                EctsBodovi = ectsBodovi,
                Rang = profil.Rang,
                Biografija = profil.Biografija ?? string.Empty,
                Vjestine = profil.Vjestine ?? string.Empty,
                Projekti = profil.Projekti ?? string.Empty,
                PreferiraneLokacije = profil.PreferiraneLokacije ?? string.Empty,
                DostupanOd = profil.DostupanOd == default ? DateTime.Today : profil.DostupanOd,
                DatumAzuriranja = profil.DatumAzuriranja,
                StatusVerifikacije = profil.StatusVerifikacije,
                IsReadOnly = isReadOnly,
                CanSendOffer = canSendOffer
            };
        }

        private static IReadOnlyList<string> SplitTags(string? value)
        {
            return (value ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IReadOnlyList<StudentProjekatViewModel> SplitProjects(string? value)
        {
            return (value ?? string.Empty)
                .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(line =>
                {
                    var parts = line.Split('|', StringSplitOptions.TrimEntries);
                    return new StudentProjekatViewModel
                    {
                        Naziv = parts.ElementAtOrDefault(0) ?? line,
                        Opis = parts.ElementAtOrDefault(1) ?? string.Empty,
                        Tehnologije = SplitTags(parts.ElementAtOrDefault(2))
                    };
                })
                .ToList();
        }
    }

    public class StudentProjekatViewModel
    {
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public IReadOnlyList<string> Tehnologije { get; set; } = [];
    }
}
