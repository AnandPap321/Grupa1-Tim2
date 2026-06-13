using ETFTalentProgram.Constants;
using ETFTalentProgram.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Data
{
    public static class DbInitializer
    {
        // ------------------------------------------------------------------ //
        // Uloge – isti niz kao originalni initializer
        // ------------------------------------------------------------------ //
        private static readonly string[] Roles =
        [
            AppRoles.Student,
            AppRoles.Firma,
            AppRoles.Referent,
            AppRoles.Administrator
        ];

        // ------------------------------------------------------------------ //
        // Seed korisnici za Identity (ApplicationUser) – isti kao original
        // ------------------------------------------------------------------ //
        private static readonly (string Email, string Password, string Role)[] SeedUsers =
        [
            ("admin@etf.ba",     "Admin123!",    AppRoles.Administrator),
            ("firma@etf.ba",     "Firma123!",    AppRoles.Firma),
            ("student@etf.ba",   "Student123!",  AppRoles.Student),
            ("referent@etf.ba",  "Referent123!", AppRoles.Referent)
        ];

        // ------------------------------------------------------------------ //
        // Lozinke za dodatne seed korisnike
        // ------------------------------------------------------------------ //
        private const string StudentPassword = "Student123!";
        private const string FirmaPassword = "Firma123!";
        private const string ReferentPassword = "Referent123!";
        private const string AdminPassword = "Admin123!";

        // ================================================================== //
        // ENTRY POINT – poziva se iz Program.cs
        // ================================================================== //
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            // 1. Uloge
            await SeedRolesAsync(roleManager);

            // 2. Osnovni Identity korisnici (original SeedUsers)
            foreach (var (email, password, role) in SeedUsers)
                await CreateUserIfNotExists(userManager, email, password, role);

            // 3. Domenski podaci
            await SeedAdministratorAsync(context, userManager);
            await SeedReferentiAsync(context, userManager);
            await SeedFirmeAsync(context, userManager);
            await SeedStudentiAsync(context, userManager);
            await SeedRangParametriAsync(context);
            await SeedOglasiAsync(context);
            await SeedPrijaveAsync(context);
            await SeedPonudeAsync(context);
            await SeedVerifikacijeAsync(context);
            await SeedLogoveAsync(context);
        }

        // ================================================================== //
        // 1. ULOGE
        // ================================================================== //
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ================================================================== //
        // HELPER – isti kao u originalnom initializer-u
        // ================================================================== //
        private static async Task<ApplicationUser?> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DatumRegistracije = DateTime.UtcNow,
                DatumZadnjePrijave = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);

            return result.Succeeded ? user : null;
        }

        // ================================================================== //
        // 2. ADMINISTRATORI
        // ================================================================== //
        private static async Task SeedAdministratorAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (await context.Administratori.AnyAsync()) return;

            // Poveži postojeći seed korisnik admin@etf.ba s domenskim entitetom
            var adminData = new[]
            {
                new { Email = "admin@etf.ba",         Ime = "Sistem",  Prezime = "Administrator", Nivo = 5 },
                new { Email = "admin2@etf.unsa.ba",   Ime = "Glavni",  Prezime = "Admin",         Nivo = 3 },
            };

            foreach (var a in adminData)
            {
                await CreateUserIfNotExists(userManager, a.Email, AdminPassword, AppRoles.Administrator);

                context.Administratori.Add(new Administrator
                {
                    Email = a.Email,
                    Lozinka = AdminPassword,
                    Uloga = Uloga.ADMINISTRATOR,
                    Status = Status.AKTIVAN,
                    DatumRegistracije = DateTime.UtcNow.AddDays(-365),
                    DatumZadnjePrijave = DateTime.UtcNow.AddHours(-1),
                    Ime = a.Ime,
                    Prezime = a.Prezime,
                    NivoOvlastenja = a.Nivo
                });
            }

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 3. REFERENTI
        // ================================================================== //
        private static async Task SeedReferentiAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (await context.Referenti.AnyAsync()) return;

            var data = new[]
            {
                new { Email = "referent@etf.ba",          Ime = "Amela",  Prezime = "Hašić", Odsjek = "Studentska služba" },
                new { Email = "kenan.basic@etf.unsa.ba",  Ime = "Kenan",  Prezime = "Bašić", Odsjek = "Studentska služba" },
                new { Email = "sanja.kovac@etf.unsa.ba",  Ime = "Sanja",  Prezime = "Kovač", Odsjek = "Referada"          },
            };

            foreach (var r in data)
            {
                await CreateUserIfNotExists(userManager, r.Email, ReferentPassword, AppRoles.Referent);

                context.Referenti.Add(new Referent
                {
                    Email = r.Email,
                    Lozinka = ReferentPassword,
                    Uloga = Uloga.REFERENT,
                    Status = Status.AKTIVAN,
                    DatumRegistracije = DateTime.UtcNow.AddDays(-300),
                    DatumZadnjePrijave = DateTime.UtcNow.AddDays(-1),
                    Ime = r.Ime,
                    Prezime = r.Prezime,
                    Odsjek = r.Odsjek
                });
            }

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 4. FIRME + FIRMA PROFILI
        // ================================================================== //
        private static async Task SeedFirmeAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (await context.Firme.AnyAsync()) return;

            var firmeData = new[]
            {
                new
                {
                    Email      = "firma@etf.ba",         // mapira na originalni SeedUser
                    Naziv      = "Mistral Technologies",
                    Opis       = "Inovativna IT kompanija fokusirana na razvoj modernih web i mobilnih aplikacija.",
                    Lokacija   = "Sarajevo, Bosna i Hercegovina",
                    Website    = "https://mistral.ba",
                    Kontakt    = "hr@mistral.ba",
                    Sektor     = "IT / Software Development",
                    Velicina   = VelicinaFirme.SREDNJA,
                    Godina     = 2015,
                    KratakOpis = "Vodimo inovaciju u regiji.",
                    PunOpis    = "Mistral Technologies je osnovan 2015. godine s ciljem pružanja visokokvalitetnih softverskih rješenja klijentima širom regije. Tim od 80+ inženjera razvija web platforme, mobilne aplikacije i cloud rješenja.",
                    Stack      = "React,Node.js,TypeScript,PostgreSQL,Docker,AWS"
                },
                new
                {
                    Email      = "careers@infinity.ba",
                    Naziv      = "Infinity Solutions",
                    Opis       = "Kompanija specijalizovana za enterprise softverska rješenja i ERP sisteme.",
                    Lokacija   = "Sarajevo, Bosna i Hercegovina",
                    Website    = "https://infinity.ba",
                    Kontakt    = "careers@infinity.ba",
                    Sektor     = "Enterprise Software",
                    Velicina   = VelicinaFirme.VELIKA,
                    Godina     = 2008,
                    KratakOpis = "Enterprise rješenja za budućnost.",
                    PunOpis    = "Infinity Solutions je jedna od vodećih IT kompanija u BiH s više od 200 zaposlenih. Pružamo ERP, CRM i custom enterprise rješenja velikim kompanijama u regiji.",
                    Stack      = "Java,Spring Boot,Oracle DB,Angular,Kubernetes,Azure"
                },
                new
                {
                    Email      = "jobs@bingo.ba",
                    Naziv      = "Bingo d.o.o.",
                    Opis       = "Vodeći maloprodajni lanac koji aktivno razvija digitalne kapacitete.",
                    Lokacija   = "Tuzla, Bosna i Hercegovina",
                    Website    = "https://bingo.ba",
                    Kontakt    = "it@bingo.ba",
                    Sektor     = "Maloprodaja / E-commerce",
                    Velicina   = VelicinaFirme.VELIKA,
                    Godina     = 1993,
                    KratakOpis = "Digitalizujemo maloprodaju.",
                    PunOpis    = "Bingo je jedan od najvećih poslodavaca u BiH. IT sektor razvija interne digitalne platforme, e-commerce rješenja i logistički softver.",
                    Stack      = "PHP,Laravel,Vue.js,MySQL,Redis"
                },
                new
                {
                    Email      = "talent@altair.ba",
                    Naziv      = "Altair Nano",
                    Opis       = "Startup fokusiran na IoT i embedded systems rješenja.",
                    Lokacija   = "Mostar, Bosna i Hercegovina",
                    Website    = "https://altair.ba",
                    Kontakt    = "talent@altair.ba",
                    Sektor     = "IoT / Embedded Systems",
                    Velicina   = VelicinaFirme.MALA,
                    Godina     = 2020,
                    KratakOpis = "Spajamo fizički i digitalni svijet.",
                    PunOpis    = "Altair Nano je mladi startup koji razvija IoT platforme i embedded firmware za pametne uređaje. Radimo s klijentima iz EU i SAD.",
                    Stack      = "C,C++,Python,MQTT,Raspberry Pi,STM32"
                },
                new
                {
                    Email      = "hr@spark.ba",
                    Naziv      = "Spark Digital",
                    Opis       = "Digitalna agencija specijalizovana za UX/UI dizajn i web razvoj.",
                    Lokacija   = "Sarajevo, Bosna i Hercegovina",
                    Website    = "https://spark.ba",
                    Kontakt    = "hr@spark.ba",
                    Sektor     = "Digital Agency",
                    Velicina   = VelicinaFirme.MALA,
                    Godina     = 2018,
                    KratakOpis = "Dizajniramo digitalna iskustva.",
                    PunOpis    = "Spark Digital je boutique digitalna agencija s 25 zaposlenih. Kombinujemo UX istraživanje, vizualni dizajn i front-end razvoj da bismo kreirali nezaboravna digitalna iskustva.",
                    Stack      = "Figma,React,Next.js,Tailwind CSS,Strapi"
                }
            };

            foreach (var f in firmeData)
            {
                await CreateUserIfNotExists(userManager, f.Email, FirmaPassword, AppRoles.Firma);

                var firma = new Firma
                {
                    Email = f.Email,
                    Lozinka = FirmaPassword,
                    Uloga = Uloga.FIRMA,
                    Status = Status.AKTIVAN,
                    DatumRegistracije = DateTime.UtcNow.AddDays(-180),
                    DatumZadnjePrijave = DateTime.UtcNow.AddDays(-3),
                    Naziv = f.Naziv,
                    OpisFirme = f.Opis,
                    Lokacija = f.Lokacija,
                    Website = f.Website,
                    KontaktEmail = f.Kontakt,
                    IndustrijskiSektor = f.Sektor,
                    VelicinaFirme = f.Velicina,
                    GodinaOsnivanja = f.Godina
                };

                context.Firme.Add(firma);
                await context.SaveChangesAsync();

                context.FirmaProfili.Add(new FirmaProfil
                {
                    FirmaId = firma.Id,
                    KratakOpis = f.KratakOpis,
                    PunOpis = f.PunOpis,
                    Lokacija = f.Lokacija,
                    Website = f.Website,
                    KontaktEmail = f.Kontakt,
                    Logotip = "",
                    TehnologijeStack = f.Stack,
                    DatumAzuriranja = DateTime.UtcNow.AddDays(-10),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN
                });

                await context.SaveChangesAsync();
            }
        }

        // ================================================================== //
        // 5. STUDENTI + STUDENT PROFILI + AKADEMSKI PODACI
        // ================================================================== //
        private static async Task SeedStudentiAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            if (await context.Studenti.AnyAsync()) return;

            var studentiData = new[]
            {
                new
                {
                    Email         = "student@etf.ba",              // mapira na originalni SeedUser
                    Ime           = "Marko",
                    Prezime       = "Petrović",
                    BrIndeksa     = "12345",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 9.2,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Student treće godine softverskog inženjerstva s interesom za web development i cloud tehnologije.",
                    Vjestine      = "React,TypeScript,Node.js,PostgreSQL,Docker",
                    Projekti      = "E-commerce platforma (React/Node.js), CLI alat za analizu logova (Python)",
                    PrefLokacije  = "Sarajevo,Remote",
                    PrefTeh       = "React,TypeScript,AWS",
                    DostupanOd    = DateTime.UtcNow.AddMonths(2),
                    Rang          = 8.7,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Web programiranje",                    Sifra = "CS301", Ocjena = 10, ECTS = 6, Sem = 5, Godina = 2024 },
                        new { Naziv = "Baze podataka",                        Sifra = "CS201", Ocjena = 9,  ECTS = 6, Sem = 3, Godina = 2023 },
                        new { Naziv = "Objektno orijentisano programiranje",  Sifra = "CS101", Ocjena = 9,  ECTS = 7, Sem = 1, Godina = 2022 },
                        new { Naziv = "Algoritmi i strukture podataka",       Sifra = "CS202", Ocjena = 10, ECTS = 6, Sem = 4, Godina = 2024 },
                        new { Naziv = "Operativni sistemi",                   Sifra = "CS203", Ocjena = 8,  ECTS = 5, Sem = 4, Godina = 2024 },
                    }
                },
                new
                {
                    Email         = "ana.kovac@etf.unsa.ba",
                    Ime           = "Ana",
                    Prezime       = "Kovač",
                    BrIndeksa     = "12346",
                    GodinaStudija = 4,
                    GodinaUpisa   = 2021,
                    Prosjek       = 9.5,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica završne godine fokusirana na backend development i DevOps. Iskustvo s cloud platformama.",
                    Vjestine      = "Java,Spring Boot,Docker,Kubernetes,PostgreSQL",
                    Projekti      = "Mikroservisna arhitektura za e-learning (Java/Spring), CI/CD pipeline (GitLab CI)",
                    PrefLokacije  = "Sarajevo",
                    PrefTeh       = "Java,Kubernetes,Azure",
                    DostupanOd    = DateTime.UtcNow.AddMonths(1),
                    Rang          = 9.1,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Distribuirani sistemi",     Sifra = "CS401", Ocjena = 10, ECTS = 6, Sem = 7, Godina = 2024 },
                        new { Naziv = "Softversko inženjerstvo",   Sifra = "CS302", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Mreže računara",            Sifra = "CS301", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Baze podataka",             Sifra = "CS201", Ocjena = 10, ECTS = 6, Sem = 3, Godina = 2022 },
                    }
                },
                new
                {
                    Email         = "emir.hasic@etf.unsa.ba",
                    Ime           = "Emir",
                    Prezime       = "Hašić",
                    BrIndeksa     = "12347",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 8.3,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Strastveni full-stack developer s iskustvom u React i Laravel projektima. Volim rad na otvorenom kodu.",
                    Vjestine      = "React,PHP,Laravel,MySQL,Git",
                    Projekti      = "Blog platforma (Laravel/React), Plugin za VS Code (TypeScript)",
                    PrefLokacije  = "Sarajevo,Mostar,Remote",
                    PrefTeh       = "React,Laravel",
                    DostupanOd    = DateTime.UtcNow.AddMonths(3),
                    Rang          = 8.3,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Web programiranje",                   Sifra = "CS301", Ocjena = 9, ECTS = 6, Sem = 5, Godina = 2024 },
                        new { Naziv = "Objektno orijentisano programiranje", Sifra = "CS101", Ocjena = 8, ECTS = 7, Sem = 1, Godina = 2022 },
                        new { Naziv = "Baze podataka",                       Sifra = "CS201", Ocjena = 8, ECTS = 6, Sem = 3, Godina = 2023 },
                    }
                },
                new
                {
                    Email         = "nina.medic@etf.unsa.ba",
                    Ime           = "Nina",
                    Prezime       = "Medić",
                    BrIndeksa     = "12348",
                    GodinaStudija = 4,
                    GodinaUpisa   = 2021,
                    Prosjek       = 8.9,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica završne godine s iskustvom u React i TypeScript projektima. Fokusirana na frontend development.",
                    Vjestine      = "React,TypeScript,CSS,Figma,Vue.js",
                    Projekti      = "Portfolio stranica (Next.js), Dashboard za analitiku (React/Chart.js)",
                    PrefLokacije  = "Sarajevo,Remote",
                    PrefTeh       = "React,TypeScript",
                    DostupanOd    = DateTime.UtcNow.AddMonths(1),
                    Rang          = 8.9,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Interakcija čovjek-računar", Sifra = "CS303", Ocjena = 10, ECTS = 5, Sem = 5, Godina = 2023 },
                        new { Naziv = "Web programiranje",           Sifra = "CS301", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Grafika i vizualizacija",     Sifra = "CS304", Ocjena = 9,  ECTS = 5, Sem = 6, Godina = 2024 },
                    }
                },
                new
                {
                    Email         = "adnan.salihovic@etf.unsa.ba",
                    Ime           = "Adnan",
                    Prezime       = "Salihovič",
                    BrIndeksa     = "12349",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 8.4,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Backend developer zainteresovan za Java ekosistem i mikroservise. Aktivni doprinioc open source projektima.",
                    Vjestine      = "Java,Spring Boot,PostgreSQL,Docker,REST API",
                    Projekti      = "API gateway za mikroservise (Spring Cloud), Sistem za upravljanje zalihama (Java/PostgreSQL)",
                    PrefLokacije  = "Sarajevo",
                    PrefTeh       = "Java,Spring Boot,PostgreSQL",
                    DostupanOd    = DateTime.UtcNow.AddMonths(2),
                    Rang          = 8.4,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Objektno orijentisano programiranje", Sifra = "CS101", Ocjena = 9, ECTS = 7, Sem = 1, Godina = 2022 },
                        new { Naziv = "Baze podataka",                       Sifra = "CS201", Ocjena = 8, ECTS = 6, Sem = 3, Godina = 2023 },
                        new { Naziv = "Algoritmi i strukture podataka",      Sifra = "CS202", Ocjena = 9, ECTS = 6, Sem = 4, Godina = 2024 },
                    }
                },
                new
                {
                    Email         = "lejla.mujic@etf.unsa.ba",
                    Ime           = "Lejla",
                    Prezime       = "Mujić",
                    BrIndeksa     = "12350",
                    GodinaStudija = 2,
                    GodinaUpisa   = 2023,
                    Prosjek       = 7.8,
                    Verificiran   = false,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica druge godine s interesom za data science i mašinsko učenje.",
                    Vjestine      = "Python,NumPy,Pandas,SQL",
                    Projekti      = "Analiza skupa podataka za predviđanje cijena nekretnina (Python/scikit-learn)",
                    PrefLokacije  = "Remote",
                    PrefTeh       = "Python,TensorFlow",
                    DostupanOd    = DateTime.UtcNow.AddMonths(6),
                    Rang          = 0.0,  // nije verificiran, rang se ne računa
                    VerStatus     = StatusVerifikacije.NA_CEKANJU,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Matematička analiza 1", Sifra = "MA101", Ocjena = 8, ECTS = 7, Sem = 1, Godina = 2023 },
                        new { Naziv = "Linearna algebra",      Sifra = "MA102", Ocjena = 7, ECTS = 6, Sem = 2, Godina = 2024 },
                    }
                },
            };

            foreach (var s in studentiData)
            {
                await CreateUserIfNotExists(userManager, s.Email, StudentPassword, AppRoles.Student);

                var student = new Student
                {
                    Email = s.Email,
                    Lozinka = StudentPassword,
                    Uloga = Uloga.STUDENT,
                    Status = s.Status,
                    DatumRegistracije = DateTime.UtcNow.AddDays(-120),
                    DatumZadnjePrijave = DateTime.UtcNow.AddDays(-2),
                    Ime = s.Ime,
                    Prezime = s.Prezime,
                    BrIndeksa = s.BrIndeksa,
                    GodinaStudija = s.GodinaStudija,
                    GodinaUpisa = s.GodinaUpisa,
                    ProsjekOcjena = s.Prosjek,
                    Verificiran = s.Verificiran
                };

                context.Studenti.Add(student);
                await context.SaveChangesAsync();

                context.StudentProfili.Add(new StudentProfil
                {
                    StudentId = student.Id,
                    Rang = s.Rang,
                    Biografija = s.Biografija,
                    Vjestine = s.Vjestine,
                    Projekti = s.Projekti,
                    PreferiraneLokacije = s.PrefLokacije,
                    PreferiraneTehnologije = s.PrefTeh,
                    DostupanOd = s.DostupanOd,
                    DatumAzuriranja = DateTime.UtcNow.AddDays(-5),
                    StatusVerifikacije = s.VerStatus
                });

                foreach (var p in s.Predmeti)
                {
                    context.AkademskiPodaci.Add(new AkademskiPodatak
                    {
                        StudentId = student.Id,
                        Predmet = p.Naziv,
                        SifraPredmeta = p.Sifra,
                        Ocjena = p.Ocjena,
                        ECTS = p.ECTS,
                        Semestar = p.Sem,
                        GodinaPolaganja = p.Godina
                    });
                }

                await context.SaveChangesAsync();
            }
        }

        // ================================================================== //
        // 6. RANG PARAMETRI
        // ================================================================== //
        private static async Task SeedRangParametriAsync(ApplicationDbContext context)
        {
            if (await context.RangParametri.AnyAsync()) return;

            context.RangParametri.Add(new RangParametri
            {
                TezinaProsjecneOcjene = 0.50,
                TezinaECTS = 0.20,
                TezinaBrojVjestina = 0.20,
                TezinaBrojProjekata = 0.10,
                Verzija = 1,
                DatumPrimjene = DateTime.UtcNow.AddDays(-200)
            });

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 7. OGLASI
        // ================================================================== //
        private static async Task SeedOglasiAsync(ApplicationDbContext context)
        {
            if (await context.Oglasi.AnyAsync()) return;

            var firme = await context.Firme.OrderBy(f => f.Id).ToListAsync();
            if (firme.Count < 5) return;

            var mistral = firme[0];
            var infinity = firme[1];
            var bingo = firme[2];
            var altair = firme[3];
            var spark = firme[4];

            context.Oglasi.AddRange(
                new Oglas
                {
                    FirmaId = mistral.Id,
                    Naslov = "Frontend Developer",
                    Opis = "Tražimo entuzijastičnog Frontend Developera koji će se pridružiti našem timu. Radit ćete na razvoju modernih React aplikacija za klijente širom regije.",
                    Tehnologije = "React,TypeScript,CSS,REST API",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.PUNO_RADNO_VRIJEME,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Sarajevo",
                    MinRang = 7.0,
                    MinProsjek = 7.5,
                    Kompenzacija = "1800-2500 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-14),
                    RokPrijave = DateTime.UtcNow.AddDays(16)
                },
                new Oglas
                {
                    FirmaId = mistral.Id,
                    Naslov = "Backend Developer (Node.js)",
                    Opis = "Rad na Node.js microservices arhitekturi s PostgreSQL bazom i Docker containerima.",
                    Tehnologije = "Node.js,PostgreSQL,Docker,REST API",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.HIBRIDNO,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Sarajevo / Remote",
                    MinRang = 7.5,
                    MinProsjek = 8.0,
                    Kompenzacija = "2000-3000 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-7),
                    RokPrijave = DateTime.UtcNow.AddDays(23)
                },
                new Oglas
                {
                    FirmaId = infinity.Id,
                    Naslov = "Java Backend Developer",
                    Opis = "Tražimo iskusnog Java developera za rad na enterprise aplikacijama. Koristimo Spring Boot, Hibernate i Oracle bazu.",
                    Tehnologije = "Java,Spring Boot,Oracle DB,Hibernate",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.PUNO_RADNO_VRIJEME,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Sarajevo",
                    MinRang = 8.0,
                    MinProsjek = 8.0,
                    Kompenzacija = "2500-3500 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-21),
                    RokPrijave = DateTime.UtcNow.AddDays(9)
                },
                new Oglas
                {
                    FirmaId = infinity.Id,
                    Naslov = "Studentska praksa – QA Inžinjer",
                    Opis = "Nudimo tromjesečnu plaćenu praksu iz oblasti osiguranja kvaliteta softvera. Naučit ćete pisati testove, koristiti Selenium i Jira alate.",
                    Tehnologije = "Selenium,Jira,SQL,Postman",
                    TipOglasa = TipOglasa.PRAKSA,
                    TipAngazmana = TipAngazmana.POLA_RADNOG_VREMENA,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Sarajevo",
                    MinRang = 6.0,
                    MinProsjek = 7.0,
                    Kompenzacija = "600 KM / mj",
                    DatumObjave = DateTime.UtcNow.AddDays(-5),
                    RokPrijave = DateTime.UtcNow.AddDays(25)
                },
                new Oglas
                {
                    FirmaId = bingo.Id,
                    Naslov = "PHP / Laravel Developer",
                    Opis = "Razvoj i održavanje internih e-commerce i logističkih rješenja. Rad u timu od 10 developera na modernoj Laravel arhitekturi.",
                    Tehnologije = "PHP,Laravel,MySQL,Vue.js",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.PUNO_RADNO_VRIJEME,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Tuzla",
                    MinRang = 7.0,
                    MinProsjek = 7.5,
                    Kompenzacija = "1600-2200 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-10),
                    RokPrijave = DateTime.UtcNow.AddDays(20)
                },
                new Oglas
                {
                    FirmaId = altair.Id,
                    Naslov = "Embedded Systems Developer (Praksa)",
                    Opis = "Idealna praksa za studente koji žele naučiti programiranje mikrokontrolera i IoT protokole. Mentorski program i mogućnost zaposlenja.",
                    Tehnologije = "C,C++,STM32,MQTT,FreeRTOS",
                    TipOglasa = TipOglasa.PRAKSA,
                    TipAngazmana = TipAngazmana.POLA_RADNOG_VREMENA,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Mostar / Remote",
                    MinRang = 6.5,
                    MinProsjek = 7.0,
                    Kompenzacija = "500 KM / mj",
                    DatumObjave = DateTime.UtcNow.AddDays(-3),
                    RokPrijave = DateTime.UtcNow.AddDays(27)
                },
                new Oglas
                {
                    FirmaId = spark.Id,
                    Naslov = "UX/UI Designer & Frontend Developer",
                    Opis = "Tražimo kreativnu osobu koja kombinuje dizajnerske vještine s frontend razvojem. Koristimo Figma za dizajn i Next.js za implementaciju.",
                    Tehnologije = "Figma,Next.js,React,Tailwind CSS",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.REMOTE,
                    StatusOglasa = StatusOglasa.AKTIVAN,
                    Lokacija = "Remote (BiH)",
                    MinRang = 7.5,
                    MinProsjek = 7.5,
                    Kompenzacija = "1800-2500 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-8),
                    RokPrijave = DateTime.UtcNow.AddDays(22)
                },
                new Oglas
                {
                    FirmaId = mistral.Id,
                    Naslov = "Junior React Developer (zatvoreno)",
                    Opis = "Pozicija je popunjena. Hvala svima koji su aplicirali.",
                    Tehnologije = "React,JavaScript",
                    TipOglasa = TipOglasa.POSAO,
                    TipAngazmana = TipAngazmana.PUNO_RADNO_VRIJEME,
                    StatusOglasa = StatusOglasa.ARHIVIRAN,
                    Lokacija = "Sarajevo",
                    MinRang = 7.0,
                    MinProsjek = 7.0,
                    Kompenzacija = "1500-2000 KM",
                    DatumObjave = DateTime.UtcNow.AddDays(-60),
                    RokPrijave = DateTime.UtcNow.AddDays(-30)
                }
            );

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 8. PRIJAVE OGLASA
        // ================================================================== //
        private static async Task SeedPrijaveAsync(ApplicationDbContext context)
        {
            if (await context.PrijaveOglasa.AnyAsync()) return;

            var studenti = await context.Studenti.OrderBy(s => s.Id).ToListAsync();
            var oglasi = await context.Oglasi
                               .Where(o => o.StatusOglasa == StatusOglasa.AKTIVAN)
                               .OrderBy(o => o.Id)
                               .ToListAsync();

            if (studenti.Count < 5 || oglasi.Count < 5) return;

            context.PrijaveOglasa.AddRange(
                new PrijavaOglas
                {
                    StudentId = studenti[0].Id,   // Marko → Frontend (Mistral)
                    OglasId = oglasi[0].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-12),
                    PropratniTekst = "Apliciram na poziciju Frontend Developera. Imam iskustvo u React i TypeScript kroz akademske i lične projekte.",
                    StatusPrijave = StatusPrijave.PREGLEDANA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-10)
                },
                new PrijavaOglas
                {
                    StudentId = studenti[0].Id,   // Marko → Backend (Mistral)
                    OglasId = oglasi[1].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-6),
                    PropratniTekst = "Zainteresovan za Backend poziciju. Solidno znanje Node.js i PostgreSQL iz ličnih projekata.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                },
                new PrijavaOglas
                {
                    StudentId = studenti[1].Id,   // Ana → Java Backend (Infinity)
                    OglasId = oglasi[2].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-18),
                    PropratniTekst = "Završavam četvrtu godinu s prosjekom 9.5. Imam iskustvo u Java/Spring Boot ekosistemu i mikroservisnoj arhitekturi.",
                    StatusPrijave = StatusPrijave.PRIHVACENA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-14)
                },
                new PrijavaOglas
                {
                    StudentId = studenti[2].Id,   // Emir → Frontend (Mistral)
                    OglasId = oglasi[0].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-9),
                    PropratniTekst = "Apliciram za Frontend poziciju. Imam iskustvo u React-u kroz rad na blog platformi i open source projektima.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                },
                new PrijavaOglas
                {
                    StudentId = studenti[3].Id,   // Nina → UX/UI (Spark)
                    OglasId = oglasi[6].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-6),
                    PropratniTekst = "Vaša pozicija je savršena kombinacija mojih vještina. Radim u Figmi i imam solidno React/TypeScript iskustvo.",
                    StatusPrijave = StatusPrijave.PREGLEDANA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-4)
                },
                new PrijavaOglas
                {
                    StudentId = studenti[4].Id,   // Adnan → Java Backend (Infinity)
                    OglasId = oglasi[2].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-20),
                    PropratniTekst = "Zainteresiran za Java Backend poziciju. Radim s Spring Boot-om i PostgreSQL-om na akademskim projektima.",
                    StatusPrijave = StatusPrijave.ODBIJENA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-16)
                },
                new PrijavaOglas
                {
                    StudentId = studenti[4].Id,   // Adnan → QA Praksa (Infinity)
                    OglasId = oglasi[3].Id,
                    DatumPrijave = DateTime.UtcNow.AddDays(-4),
                    PropratniTekst = "Apliciram za QA praksu. Voljan naučiti testiranje softvera u profesionalnom okruženju.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                }
            );

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 9. PONUDE
        // ================================================================== //
        private static async Task SeedPonudeAsync(ApplicationDbContext context)
        {
            if (await context.Ponude.AnyAsync()) return;

            var studenti = await context.Studenti.OrderBy(s => s.Id).ToListAsync();
            var firme = await context.Firme.OrderBy(f => f.Id).ToListAsync();

            if (studenti.Count < 5 || firme.Count < 5) return;

            context.Ponude.AddRange(
                new Ponuda
                {
                    PosiljalacId = firme[0].Id,     // Mistral → Marko
                    PrimalacId = studenti[0].Id,
                    TekstPoruke = "Poštovani Marko, pregledali smo vaš profil i zainteresovani smo za saradnju na poziciji Frontend Developer. Pozivamo vas na razgovor.",
                    DatumSlanja = DateTime.UtcNow.AddDays(-11),
                    Status = StatusPonude.NA_CEKANJU,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = null
                },
                new Ponuda
                {
                    PosiljalacId = firme[1].Id,     // Infinity → Ana
                    PrimalacId = studenti[1].Id,
                    TekstPoruke = "Poštovana Ana, vaš profil odgovara našim zahtjevima za Java Backend poziciju. Pozivamo vas na tehničku evaluaciju.",
                    DatumSlanja = DateTime.UtcNow.AddDays(-9),
                    Status = StatusPonude.PRIHVACENO,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-7)
                },
                new Ponuda
                {
                    PosiljalacId = firme[4].Id,     // Spark → Nina
                    PrimalacId = studenti[3].Id,
                    TekstPoruke = "Poštovana Nina, vaš portfolio nas je impresionirao. Nudimo vam poziciju UX/UI Developer u Spark Digitalu, remote rad i fleksibilno radno vrijeme.",
                    DatumSlanja = DateTime.UtcNow.AddDays(-5),
                    Status = StatusPonude.NA_CEKANJU,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = null
                },
                new Ponuda
                {
                    PosiljalacId = firme[3].Id,     // Altair → Adnan
                    PrimalacId = studenti[4].Id,
                    TekstPoruke = "Poštovani Adnan, nudimo vam plaćenu praksu iz embedded systems programiranja. Idealna za studente koji žele raditi s hardverom i IoT tehnologijama.",
                    DatumSlanja = DateTime.UtcNow.AddDays(-3),
                    Status = StatusPonude.ODBIJENO,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-1)
                },
                new Ponuda
                {
                    PosiljalacId = firme[0].Id,     // Mistral → Emir
                    PrimalacId = studenti[2].Id,
                    TekstPoruke = "Poštovani Emir, zainteresovani smo za vaše Laravel i React vještine. Nudimo junior poziciju s mentorskim programom.",
                    DatumSlanja = DateTime.UtcNow.AddDays(-2),
                    Status = StatusPonude.POSLANO,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = null
                }
            );

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 10. VERIFIKACIJE
        // ================================================================== //
        private static async Task SeedVerifikacijeAsync(ApplicationDbContext context)
        {
            if (await context.Verifikacije.AnyAsync()) return;

            var studenti = await context.Studenti.OrderBy(s => s.Id).ToListAsync();
            var referenti = await context.Referenti.OrderBy(r => r.Id).ToListAsync();

            if (studenti.Count < 6 || referenti.Count < 2) return;

            context.Verifikacije.AddRange(
                new Verifikacija
                {
                    StudentId = studenti[0].Id,
                    ReferentId = referenti[0].Id,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-90),
                    DatumVerifikacije = DateTime.UtcNow.AddDays(-85),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN,
                    Komentar = "Podaci potvrđeni uvidom u zvanične evidencije fakulteta.",
                    Dokumenti = ""
                },
                new Verifikacija
                {
                    StudentId = studenti[1].Id,
                    ReferentId = referenti[0].Id,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-70),
                    DatumVerifikacije = DateTime.UtcNow.AddDays(-65),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN,
                    Komentar = "Akademski podaci verificirani. Prosjek i ECTS su tačni.",
                    Dokumenti = ""
                },
                new Verifikacija
                {
                    StudentId = studenti[2].Id,
                    ReferentId = referenti[1].Id,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-60),
                    DatumVerifikacije = DateTime.UtcNow.AddDays(-55),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN,
                    Komentar = "Verificirano bez primjedbi.",
                    Dokumenti = ""
                },
                new Verifikacija
                {
                    StudentId = studenti[3].Id,
                    ReferentId = referenti[0].Id,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-50),
                    DatumVerifikacije = DateTime.UtcNow.AddDays(-45),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN,
                    Komentar = "Sve u redu, podaci odgovaraju evidenciji.",
                    Dokumenti = ""
                },
                new Verifikacija
                {
                    StudentId = studenti[4].Id,
                    ReferentId = referenti[1].Id,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-40),
                    DatumVerifikacije = DateTime.UtcNow.AddDays(-35),
                    StatusVerifikacije = StatusVerifikacije.VERIFICIRAN,
                    Komentar = "Podaci potvrđeni.",
                    Dokumenti = ""
                },
                new Verifikacija   // Lejla – na čekanju
                {
                    StudentId = studenti[5].Id,
                    ReferentId = null,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-5),
                    DatumVerifikacije = null,
                    StatusVerifikacije = StatusVerifikacije.NA_CEKANJU,
                    Komentar = null,
                    Dokumenti = ""
                }
            );

            await context.SaveChangesAsync();
        }

        // ================================================================== //
        // 11. LOGOVI
        // ================================================================== //
        private static async Task SeedLogoveAsync(ApplicationDbContext context)
        {
            if (await context.Logovi.AnyAsync()) return;

            var studenti = await context.Studenti.OrderBy(s => s.Id).ToListAsync();
            var firme = await context.Firme.OrderBy(f => f.Id).ToListAsync();

            context.Logovi.AddRange(
                new Log { TipAkcije = "REGISTRACIJA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-120), KorisnikId = studenti.Count > 0 ? studenti[0].Id : null, IpAdresa = "192.168.1.10", Detalji = "Novi student registrovan: student@etf.ba" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-2), KorisnikId = studenti.Count > 0 ? studenti[0].Id : null, IpAdresa = "192.168.1.10", Detalji = "Uspješna prijava: student@etf.ba" },
                new Log { TipAkcije = "VERIFIKACIJA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-85), KorisnikId = studenti.Count > 0 ? studenti[0].Id : null, IpAdresa = "192.168.1.20", Detalji = "Verifikacija odobrena za studenta: student@etf.ba" },
                new Log { TipAkcije = "VERIFIKACIJA", Nivo = NivoLoga.WARNING, VrijemeAkcije = DateTime.UtcNow.AddDays(-35), KorisnikId = null, IpAdresa = "192.168.1.20", Detalji = "Verifikacija odbijena – neispravni podaci" },
                new Log { TipAkcije = "OGLAS", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-14), KorisnikId = firme.Count > 0 ? firme[0].Id : null, IpAdresa = "10.0.0.5", Detalji = "Novi oglas kreiran: Frontend Developer (Mistral Technologies)" },
                new Log { TipAkcije = "PRIJAVA_OGLAS", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-12), KorisnikId = studenti.Count > 0 ? studenti[0].Id : null, IpAdresa = "192.168.1.10", Detalji = "Student aplicirao na oglas: Frontend Developer" },
                new Log { TipAkcije = "SLANJE_EMAIL", Nivo = NivoLoga.ERROR, VrijemeAkcije = DateTime.UtcNow.AddDays(-11), KorisnikId = null, IpAdresa = "127.0.0.1", Detalji = "Slanje email notifikacije nije uspjelo: connection timeout" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.WARNING, VrijemeAkcije = DateTime.UtcNow.AddDays(-8), KorisnikId = null, IpAdresa = "10.0.0.99", Detalji = "Neuspješan pokušaj prijave: nepostojeci@test.com" },
                new Log { TipAkcije = "PONUDA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-5), KorisnikId = firme.Count > 4 ? firme[4].Id : null, IpAdresa = "10.0.0.7", Detalji = "Ponuda poslana od Spark Digital" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddHours(-3), KorisnikId = firme.Count > 0 ? firme[0].Id : null, IpAdresa = "10.0.0.5", Detalji = "Uspješna prijava: firma@etf.ba" }
            );

            await context.SaveChangesAsync();
        }
    }
}
