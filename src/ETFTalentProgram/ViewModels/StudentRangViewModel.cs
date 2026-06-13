namespace ETFTalentProgram.ViewModels
{
    public class StudentRangViewModel
    {
        public long StudentId { get; set; }
        public string ImePrezime { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BrojIndeksa { get; set; } = string.Empty;
        public double ProsjekOcjena { get; set; }
        public int BrojPolozenihPredmeta { get; set; }
        public int UkupnoEcts { get; set; }
        public int BrojProjekata { get; set; }
        public string Vjestine { get; set; } = string.Empty;
        public string PreferiraneTehnologije { get; set; } = string.Empty;
        public double Rang { get; set; }
    }
}
