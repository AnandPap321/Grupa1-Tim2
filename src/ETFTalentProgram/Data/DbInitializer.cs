using ETFTalentProgram.Constants;
using ETFTalentProgram.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ETFTalentProgram.Data
{
    public static class DbInitializer
    {
        private static readonly string[] Roles =
        [
            AppRoles.Student,
            AppRoles.Firma,
            AppRoles.Referent,
            AppRoles.Administrator
        ];

        private static readonly (string Email, string Password, string Role)[] SeedUsers =
        [
            ("admin@etf.ba",    "Admin123!",    AppRoles.Administrator),
            ("firma@etf.ba",    "Firma123!",    AppRoles.Firma),
            ("student@etf.ba",  "Student123!",  AppRoles.Student),
            ("referent@etf.ba", "Referent123!", AppRoles.Referent)
        ];

        private const string StudentPassword = "Student123!";
        private const string FirmaPassword = "Firma123!";
        private const string ReferentPassword = "Referent123!";
        private const string AdminPassword = "Admin123!";

        // ================================================================== //
        // ENTRY POINT
        // ================================================================== //
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);

            foreach (var (email, password, role) in SeedUsers)
                await CreateUserIfNotExists(userManager, email, password, role);

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
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ================================================================== //
        // HELPER
        // ================================================================== //
        private static async Task<ApplicationUser?> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string role)
        {
            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null) return existing;

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

            var adminData = new[]
            {
                new { Email = "admin@etf.ba",       Ime = "Sistem", Prezime = "Administrator", Nivo = 5 },
                new { Email = "admin2@etf.unsa.ba", Ime = "Glavni", Prezime = "Admin",         Nivo = 3 },
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
                new { Email = "referent@etf.ba",         Ime = "Amela", Prezime = "Hasic", Odsjek = "Studentska sluzba" },
                new { Email = "kenan.basic@etf.unsa.ba", Ime = "Kenan", Prezime = "Basic", Odsjek = "Studentska sluzba" },
                new { Email = "sanja.kovac@etf.unsa.ba", Ime = "Sanja", Prezime = "Kovac", Odsjek = "Referada"          },
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
                    Email      = "firma@etf.ba",
                    Naziv      = "Mistral Technologies",
                    Opis       = "Inovativna IT kompanija fokusirana na razvoj modernih web i mobilnih aplikacija.",
                    Lokacija   = "Sarajevo, Bosna i Hercegovina",
                    Website    = "https://mistral.ba",
                    Kontakt    = "hr@mistral.ba",
                    Sektor     = "IT / Software Development",
                    Velicina   = VelicinaFirme.SREDNJA,
                    Godina     = 2015,
                    KratakOpis = "Vodimo inovaciju u regiji.",
                    PunOpis    = "Mistral Technologies je osnovan 2015. godine s ciljem pruzanja visokokvalitetnih softverskih rjesenja klijentima sirom regije.",
                    Stack      = "React,Node.js,TypeScript,PostgreSQL,Docker,AWS"
                },
                new
                {
                    Email      = "careers@infinity.ba",
                    Naziv      = "Infinity Solutions",
                    Opis       = "Kompanija specijalizovana za enterprise softverska rjesenja i ERP sisteme.",
                    Lokacija   = "Sarajevo, Bosna i Hercegovina",
                    Website    = "https://infinity.ba",
                    Kontakt    = "careers@infinity.ba",
                    Sektor     = "Enterprise Software",
                    Velicina   = VelicinaFirme.VELIKA,
                    Godina     = 2008,
                    KratakOpis = "Enterprise rjesenja za buducnost.",
                    PunOpis    = "Infinity Solutions je jedna od vodecih IT kompanija u BiH s vise od 200 zaposlenih.",
                    Stack      = "Java,Spring Boot,Oracle DB,Angular,Kubernetes,Azure"
                },
                new
                {
                    Email      = "jobs@bingo.ba",
                    Naziv      = "Bingo d.o.o.",
                    Opis       = "Vodeci maloprodajni lanac koji aktivno razvija digitalne kapacitete.",
                    Lokacija   = "Tuzla, Bosna i Hercegovina",
                    Website    = "https://bingo.ba",
                    Kontakt    = "it@bingo.ba",
                    Sektor     = "Maloprodaja / E-commerce",
                    Velicina   = VelicinaFirme.VELIKA,
                    Godina     = 1993,
                    KratakOpis = "Digitalizujemo maloprodaju.",
                    PunOpis    = "Bingo je jedan od najvecih poslodavaca u BiH. IT sektor razvija interne digitalne platforme i logisticki softver.",
                    Stack      = "PHP,Laravel,Vue.js,MySQL,Redis"
                },
                new
                {
                    Email      = "talent@altair.ba",
                    Naziv      = "Altair Nano",
                    Opis       = "Startup fokusiran na IoT i embedded systems rjesenja.",
                    Lokacija   = "Mostar, Bosna i Hercegovina",
                    Website    = "https://altair.ba",
                    Kontakt    = "talent@altair.ba",
                    Sektor     = "IoT / Embedded Systems",
                    Velicina   = VelicinaFirme.MALA,
                    Godina     = 2020,
                    KratakOpis = "Spajamo fizicki i digitalni svijet.",
                    PunOpis    = "Altair Nano razvija IoT platforme i embedded firmware za pametne uredaje.",
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
                    PunOpis    = "Spark Digital kombinuje UX istrazivanje, vizualni dizajn i front-end razvoj.",
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
                    Email         = "student@etf.ba",
                    Ime           = "Marko",
                    Prezime       = "Petrovic",
                    BrIndeksa     = "12345",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 9.2,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Student trece godine softverskog inzenjerstva s interesom za web development i cloud tehnologije.",
                    Vjestine      = "React,TypeScript,Node.js,PostgreSQL,Docker",
                    Projekti      = "E-commerce platforma (React/Node.js), CLI alat za analizu logova (Python)",
                    PrefLokacije  = "Sarajevo,Remote",
                    PrefTeh       = "React,TypeScript,AWS",
                    DostupanOd    = DateTime.UtcNow.AddMonths(2),
                    Rang          = 8.7,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Web programiranje",                   Sifra = "CS301", Ocjena = 10, ECTS = 6, Sem = 5, Godina = 2024 },
                        new { Naziv = "Baze podataka",                       Sifra = "CS201", Ocjena = 9,  ECTS = 6, Sem = 3, Godina = 2023 },
                        new { Naziv = "Objektno orijentisano programiranje", Sifra = "CS101", Ocjena = 9,  ECTS = 7, Sem = 1, Godina = 2022 },
                        new { Naziv = "Algoritmi i strukture podataka",      Sifra = "CS202", Ocjena = 10, ECTS = 6, Sem = 4, Godina = 2024 },
                        new { Naziv = "Operativni sistemi",                  Sifra = "CS203", Ocjena = 8,  ECTS = 5, Sem = 4, Godina = 2024 },
                    }
                },
                new
                {
                    Email         = "ana.kovac@etf.unsa.ba",
                    Ime           = "Ana",
                    Prezime       = "Kovac",
                    BrIndeksa     = "12346",
                    GodinaStudija = 4,
                    GodinaUpisa   = 2021,
                    Prosjek       = 9.5,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica zavrsne godine fokusirana na backend development i DevOps.",
                    Vjestine      = "Java,Spring Boot,Docker,Kubernetes,PostgreSQL",
                    Projekti      = "Mikroservisna arhitektura za e-learning (Java/Spring), CI/CD pipeline (GitLab CI)",
                    PrefLokacije  = "Sarajevo",
                    PrefTeh       = "Java,Kubernetes,Azure",
                    DostupanOd    = DateTime.UtcNow.AddMonths(1),
                    Rang          = 9.1,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Distribuirani sistemi",   Sifra = "CS401", Ocjena = 10, ECTS = 6, Sem = 7, Godina = 2024 },
                        new { Naziv = "Softversko inzenjerstvo", Sifra = "CS302", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Mreze racunara",          Sifra = "CS301", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Baze podataka",           Sifra = "CS201", Ocjena = 10, ECTS = 6, Sem = 3, Godina = 2022 },
                    }
                },
                new
                {
                    Email         = "emir.hasic@etf.unsa.ba",
                    Ime           = "Emir",
                    Prezime       = "Hasic",
                    BrIndeksa     = "12347",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 8.3,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Strastveni full-stack developer s iskustvom u React i Laravel projektima.",
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
                    Prezime       = "Medic",
                    BrIndeksa     = "12348",
                    GodinaStudija = 4,
                    GodinaUpisa   = 2021,
                    Prosjek       = 8.9,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica zavrsne godine s iskustvom u React i TypeScript projektima.",
                    Vjestine      = "React,TypeScript,CSS,Figma,Vue.js",
                    Projekti      = "Portfolio stranica (Next.js), Dashboard za analitiku (React/Chart.js)",
                    PrefLokacije  = "Sarajevo,Remote",
                    PrefTeh       = "React,TypeScript",
                    DostupanOd    = DateTime.UtcNow.AddMonths(1),
                    Rang          = 8.9,
                    VerStatus     = StatusVerifikacije.VERIFICIRAN,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Interakcija covjek-racunar", Sifra = "CS303", Ocjena = 10, ECTS = 5, Sem = 5, Godina = 2023 },
                        new { Naziv = "Web programiranje",           Sifra = "CS301", Ocjena = 9,  ECTS = 6, Sem = 5, Godina = 2023 },
                        new { Naziv = "Grafika i vizualizacija",     Sifra = "CS304", Ocjena = 9,  ECTS = 5, Sem = 6, Godina = 2024 },
                    }
                },
                new
                {
                    Email         = "adnan.salihovic@etf.unsa.ba",
                    Ime           = "Adnan",
                    Prezime       = "Salihovic",
                    BrIndeksa     = "12349",
                    GodinaStudija = 3,
                    GodinaUpisa   = 2022,
                    Prosjek       = 8.4,
                    Verificiran   = true,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Backend developer zainteresovan za Java ekosistem i mikroservise.",
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
                    Prezime       = "Mujic",
                    BrIndeksa     = "12350",
                    GodinaStudija = 2,
                    GodinaUpisa   = 2023,
                    Prosjek       = 7.8,
                    Verificiran   = false,
                    Status        = Status.AKTIVAN,
                    Biografija    = "Studentica druge godine s interesom za data science i masinsko ucenje.",
                    Vjestine      = "Python,NumPy,Pandas,SQL",
                    Projekti      = "Analiza skupa podataka za predvidanje cijena nekretnina (Python/scikit-learn)",
                    PrefLokacije  = "Remote",
                    PrefTeh       = "Python,TensorFlow",
                    DostupanOd    = DateTime.UtcNow.AddMonths(6),
                    Rang          = 0.0,
                    VerStatus     = StatusVerifikacije.NA_CEKANJU,
                    Predmeti      = new[]
                    {
                        new { Naziv = "Matematicka analiza 1", Sifra = "MA101", Ocjena = 8, ECTS = 7, Sem = 1, Godina = 2023 },
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
        // FIX: dohvat po emailu umjesto po indeksu
        // ================================================================== //
        private static async Task SeedOglasiAsync(ApplicationDbContext context)
        {
            if (await context.Oglasi.AnyAsync()) return;

            var firmeMap = await context.Firme
                .ToDictionaryAsync(f => f.Email, f => f.Id);

            if (firmeMap.Count == 0)
            {
                Console.WriteLine("[SEED] UPOZORENJE: Nema firmi u bazi, oglasi se preskacaju.");
                return;
            }

            long GetFirmaId(string email) =>
                firmeMap.TryGetValue(email, out var id) ? id : firmeMap.Values.First();

            context.Oglasi.AddRange(
                new Oglas
                {
                    FirmaId = GetFirmaId("firma@etf.ba"),
                    Naslov = "Frontend Developer",
                    Opis = "Trazimo entuzijasticnog Frontend Developera koji ce se prikljuciti nasem timu. Radit cete na razvoju modernih React aplikacija za klijente sirom regije.",
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
                    FirmaId = GetFirmaId("firma@etf.ba"),
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
                    FirmaId = GetFirmaId("careers@infinity.ba"),
                    Naslov = "Java Backend Developer",
                    Opis = "Trazimo iskusnog Java developera za rad na enterprise aplikacijama. Koristimo Spring Boot, Hibernate i Oracle bazu.",
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
                    FirmaId = GetFirmaId("careers@infinity.ba"),
                    Naslov = "Studentska praksa - QA Inzinjer",
                    Opis = "Nudimo tromjesecnu placenu praksu iz oblasti osiguranja kvaliteta softvera.",
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
                    FirmaId = GetFirmaId("jobs@bingo.ba"),
                    Naslov = "PHP / Laravel Developer",
                    Opis = "Razvoj i odrzavanje internih e-commerce i logistickih rjesenja.",
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
                    FirmaId = GetFirmaId("talent@altair.ba"),
                    Naslov = "Embedded Systems Developer (Praksa)",
                    Opis = "Idealna praksa za studente koji zele nauciti programiranje mikrokontrolera i IoT protokole.",
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
                    FirmaId = GetFirmaId("hr@spark.ba"),
                    Naslov = "UX/UI Designer i Frontend Developer",
                    Opis = "Trazimo kreativnu osobu koja kombinuje dizajnerske vjestine s frontend razvojem.",
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
                    FirmaId = GetFirmaId("firma@etf.ba"),
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
        // FIX: dohvat po emailu/naslovu umjesto po indeksu
        // ================================================================== //
        private static async Task SeedPrijaveAsync(ApplicationDbContext context)
        {
            if (await context.PrijaveOglasa.AnyAsync()) return;

            var studentiMap = await context.Studenti
                .ToDictionaryAsync(s => s.Email, s => s.Id);

            var oglasiMap = await context.Oglasi
                .Where(o => o.StatusOglasa == StatusOglasa.AKTIVAN)
                .ToDictionaryAsync(o => o.Naslov, o => o.Id);

            if (studentiMap.Count == 0 || oglasiMap.Count == 0)
            {
                Console.WriteLine("[SEED] UPOZORENJE: Nema studenata ili oglasa, prijave se preskacaju.");
                return;
            }

            bool TryStudent(string email, out long id) =>
                studentiMap.TryGetValue(email, out id);
            bool TryOglas(string naslov, out long id) =>
                oglasiMap.TryGetValue(naslov, out id);

            var prijave = new List<PrijavaOglas>();

            if (TryStudent("student@etf.ba", out var markoId) &&
                TryOglas("Frontend Developer", out var frontendId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = markoId,
                    OglasId = frontendId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-12),
                    PropratniTekst = "Apliciram na poziciju Frontend Developera. Imam iskustvo u React i TypeScript kroz akademske i licne projekte.",
                    StatusPrijave = StatusPrijave.PREGLEDANA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-10)
                });

            if (TryStudent("student@etf.ba", out markoId) &&
                TryOglas("Backend Developer (Node.js)", out var backendId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = markoId,
                    OglasId = backendId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-6),
                    PropratniTekst = "Zainteresovan za Backend poziciju. Solidno znanje Node.js i PostgreSQL iz licnih projekata.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                });

            if (TryStudent("ana.kovac@etf.unsa.ba", out var anaId) &&
                TryOglas("Java Backend Developer", out var javaId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = anaId,
                    OglasId = javaId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-18),
                    PropratniTekst = "Zavrsavam cetvrtu godinu s prosjekom 9.5. Imam iskustvo u Java/Spring Boot i mikroservisnoj arhitekturi.",
                    StatusPrijave = StatusPrijave.PRIHVACENA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-14)
                });

            if (TryStudent("emir.hasic@etf.unsa.ba", out var emirId) &&
                TryOglas("Frontend Developer", out frontendId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = emirId,
                    OglasId = frontendId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-9),
                    PropratniTekst = "Apliciram za Frontend poziciju. Imam iskustvo u React-u kroz rad na blog platformi.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                });

            if (TryStudent("nina.medic@etf.unsa.ba", out var ninaId) &&
                TryOglas("UX/UI Designer i Frontend Developer", out var uxId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = ninaId,
                    OglasId = uxId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-6),
                    PropratniTekst = "Vasa pozicija je savrsena kombinacija mojih vjestina. Radim u Figmi i imam React/TypeScript iskustvo.",
                    StatusPrijave = StatusPrijave.PREGLEDANA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-4)
                });

            if (TryStudent("adnan.salihovic@etf.unsa.ba", out var adnanId) &&
                TryOglas("Java Backend Developer", out javaId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = adnanId,
                    OglasId = javaId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-20),
                    PropratniTekst = "Zainteresiran za Java Backend poziciju. Radim s Spring Boot-om i PostgreSQL-om.",
                    StatusPrijave = StatusPrijave.ODBIJENA,
                    DatumOdgovora = DateTime.UtcNow.AddDays(-16)
                });

            if (TryStudent("adnan.salihovic@etf.unsa.ba", out adnanId) &&
                TryOglas("Studentska praksa - QA Inzinjer", out var qaId))
                prijave.Add(new PrijavaOglas
                {
                    StudentId = adnanId,
                    OglasId = qaId,
                    DatumPrijave = DateTime.UtcNow.AddDays(-4),
                    PropratniTekst = "Apliciram za QA praksu. Voljan nauciti testiranje softvera u profesionalnom okruzenju.",
                    StatusPrijave = StatusPrijave.NOVA,
                    DatumOdgovora = null
                });

            if (prijave.Count > 0)
            {
                context.PrijaveOglasa.AddRange(prijave);
                await context.SaveChangesAsync();
            }
        }

        // ================================================================== //
        // 9. PONUDE
        // FIX: TPC nasljedivanje - Firma.Id == Korisnik.Id (shared PK)
        // PosiljalacId/PrimalacId su FK na Korisnik hijerarhiju,
        // a Firma.Id i Student.Id su direktno taj isti ID.
        // ================================================================== //
        private static async Task SeedPonudeAsync(ApplicationDbContext context)
        {
            if (await context.Ponude.AnyAsync()) return;

            var firmeMap = await context.Firme.ToDictionaryAsync(f => f.Email, f => f.Id);
            var studentiMap = await context.Studenti.ToDictionaryAsync(s => s.Email, s => s.Id);

            if (firmeMap.Count == 0 || studentiMap.Count == 0)
            {
                Console.WriteLine("[SEED] UPOZORENJE: Nema firmi ili studenata, ponude se preskacaju.");
                return;
            }

            var ponude = new List<Ponuda>();

            void DodajPonudu(string firmaEmail, string studentEmail,
                             string tekst, StatusPonude status,
                             int sentDaysAgo, DateTime? odgovor)
            {
                if (!firmeMap.TryGetValue(firmaEmail, out var firmaId)) return;
                if (!studentiMap.TryGetValue(studentEmail, out var studId)) return;

                ponude.Add(new Ponuda
                {
                    PosiljalacId = firmaId,   // Firma.Id == Korisnik.Id (TPC shared PK)
                    PrimalacId = studId,    // Student.Id == Korisnik.Id (TPC shared PK)
                    TekstPoruke = tekst,
                    DatumSlanja = DateTime.UtcNow.AddDays(-sentDaysAgo),
                    Status = status,
                    TipPonude = TipPonude.FIRMA_STUDENTU,
                    DatumOdgovora = odgovor
                });
            }

            DodajPonudu("firma@etf.ba", "student@etf.ba",
                "Postovani Marko, pregledali smo vas profil i zainteresovani smo za saradnju na poziciji Frontend Developer. Pozivamo vas na razgovor.",
                StatusPonude.NA_CEKANJU, 11, null);

            DodajPonudu("careers@infinity.ba", "ana.kovac@etf.unsa.ba",
                "Postovana Ana, vas profil odgovara nasim zahtjevima za Java Backend poziciju. Pozivamo vas na tehnicku evaluaciju.",
                StatusPonude.PRIHVACENO, 9, DateTime.UtcNow.AddDays(-7));

            DodajPonudu("hr@spark.ba", "nina.medic@etf.unsa.ba",
                "Postovana Nina, vas portfolio nas je impresionirao. Nudimo vam poziciju UX/UI Developer u Spark Digitalu, remote rad i fleksibilno radno vrijeme.",
                StatusPonude.NA_CEKANJU, 5, null);

            DodajPonudu("talent@altair.ba", "adnan.salihovic@etf.unsa.ba",
                "Postovani Adnan, nudimo vam placenu praksu iz embedded systems programiranja.",
                StatusPonude.ODBIJENO, 3, DateTime.UtcNow.AddDays(-1));

            DodajPonudu("firma@etf.ba", "emir.hasic@etf.unsa.ba",
                "Postovani Emir, zainteresovani smo za vase Laravel i React vjestine. Nudimo junior poziciju s mentorskim programom.",
                StatusPonude.POSLANO, 2, null);

            if (ponude.Count > 0)
            {
                context.Ponude.AddRange(ponude);
                await context.SaveChangesAsync();
            }
        }

        // ================================================================== //
        // 10. VERIFIKACIJE
        // FIX: dohvat po emailu, relaxed guardovi, nullable referent
        // ================================================================== //
        private static async Task SeedVerifikacijeAsync(ApplicationDbContext context)
        {
            if (await context.Verifikacije.AnyAsync()) return;

            var studentiMap = await context.Studenti.ToDictionaryAsync(s => s.Email, s => s.Id);
            var referentiMap = await context.Referenti.ToDictionaryAsync(r => r.Email, r => r.Id);

            if (studentiMap.Count == 0)
            {
                Console.WriteLine("[SEED] UPOZORENJE: Nema studenata, verifikacije se preskacaju.");
                return;
            }

            // Uzmi dostupne referente po emailu, uz fallback na prvog/drugog
            referentiMap.TryGetValue("referent@etf.ba", out var ref1Id);
            referentiMap.TryGetValue("kenan.basic@etf.unsa.ba", out var ref2Id);

            // Ako specificni ne postoje, uzmi prvog dostupnog
            if (ref1Id == 0) ref1Id = referentiMap.Values.FirstOrDefault();
            if (ref2Id == 0) ref2Id = referentiMap.Values.Skip(1).FirstOrDefault();

            long? Ref1 = ref1Id != 0 ? ref1Id : null;
            long? Ref2 = ref2Id != 0 ? ref2Id : Ref1; // fallback na prvog

            var verifikacije = new List<Verifikacija>();

            void Dodaj(string studentEmail, long? referentId,
                       StatusVerifikacije status, string? komentar,
                       int daysAgo, int? resolvedDaysAgo)
            {
                if (!studentiMap.TryGetValue(studentEmail, out var studId)) return;

                verifikacije.Add(new Verifikacija
                {
                    StudentId = studId,
                    ReferentId = referentId,
                    DatumPodnosenja = DateTime.UtcNow.AddDays(-daysAgo),
                    DatumVerifikacije = resolvedDaysAgo.HasValue
                                            ? DateTime.UtcNow.AddDays(-resolvedDaysAgo.Value)
                                            : null,
                    StatusVerifikacije = status,
                    Komentar = komentar,
                    Dokumenti = ""
                });
            }

            Dodaj("student@etf.ba", Ref1, StatusVerifikacije.VERIFICIRAN, "Podaci potvrdjeni uvidom u zvanicne evidencije fakulteta.", 90, 85);
            Dodaj("ana.kovac@etf.unsa.ba", Ref1, StatusVerifikacije.VERIFICIRAN, "Akademski podaci verificirani. Prosjek i ECTS su tacni.", 70, 65);
            Dodaj("emir.hasic@etf.unsa.ba", Ref2, StatusVerifikacije.VERIFICIRAN, "Verificirano bez primjedbi.", 60, 55);
            Dodaj("nina.medic@etf.unsa.ba", Ref1, StatusVerifikacije.VERIFICIRAN, "Sve u redu, podaci odgovaraju evidenciji.", 50, 45);
            Dodaj("adnan.salihovic@etf.unsa.ba", Ref2, StatusVerifikacije.VERIFICIRAN, "Podaci potvrdjeni.", 40, 35);
            Dodaj("lejla.mujic@etf.unsa.ba", null, StatusVerifikacije.NA_CEKANJU, null, 5, null);

            if (verifikacije.Count > 0)
            {
                context.Verifikacije.AddRange(verifikacije);
                await context.SaveChangesAsync();
            }
        }

        // ================================================================== //
        // 11. LOGOVI
        // ================================================================== //
        private static async Task SeedLogoveAsync(ApplicationDbContext context)
        {
            if (await context.Logovi.AnyAsync()) return;

            var studentiMap = await context.Studenti.ToDictionaryAsync(s => s.Email, s => s.Id);
            var firmeMap = await context.Firme.ToDictionaryAsync(f => f.Email, f => f.Id);

            studentiMap.TryGetValue("student@etf.ba", out var markoId);
            firmeMap.TryGetValue("firma@etf.ba", out var mistralId);
            firmeMap.TryGetValue("hr@spark.ba", out var sparkId);

            context.Logovi.AddRange(
                new Log { TipAkcije = "REGISTRACIJA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-120), KorisnikId = markoId != 0 ? markoId : null, IpAdresa = "192.168.1.10", Detalji = "Novi student registrovan: student@etf.ba" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-2), KorisnikId = markoId != 0 ? markoId : null, IpAdresa = "192.168.1.10", Detalji = "Uspjesna prijava: student@etf.ba" },
                new Log { TipAkcije = "VERIFIKACIJA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-85), KorisnikId = markoId != 0 ? markoId : null, IpAdresa = "192.168.1.20", Detalji = "Verifikacija odobrena za studenta: student@etf.ba" },
                new Log { TipAkcije = "VERIFIKACIJA", Nivo = NivoLoga.WARNING, VrijemeAkcije = DateTime.UtcNow.AddDays(-35), KorisnikId = null, IpAdresa = "192.168.1.20", Detalji = "Verifikacija odbijena - neispravni podaci" },
                new Log { TipAkcije = "OGLAS", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-14), KorisnikId = mistralId != 0 ? mistralId : null, IpAdresa = "10.0.0.5", Detalji = "Novi oglas kreiran: Frontend Developer (Mistral Technologies)" },
                new Log { TipAkcije = "PRIJAVA_OGLAS", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-12), KorisnikId = markoId != 0 ? markoId : null, IpAdresa = "192.168.1.10", Detalji = "Student aplicirao na oglas: Frontend Developer" },
                new Log { TipAkcije = "SLANJE_EMAIL", Nivo = NivoLoga.ERROR, VrijemeAkcije = DateTime.UtcNow.AddDays(-11), KorisnikId = null, IpAdresa = "127.0.0.1", Detalji = "Slanje email notifikacije nije uspjelo: connection timeout" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.WARNING, VrijemeAkcije = DateTime.UtcNow.AddDays(-8), KorisnikId = null, IpAdresa = "10.0.0.99", Detalji = "Neuspjesan pokusaj prijave: nepostojeci@test.com" },
                new Log { TipAkcije = "PONUDA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddDays(-5), KorisnikId = sparkId != 0 ? sparkId : null, IpAdresa = "10.0.0.7", Detalji = "Ponuda poslana od Spark Digital" },
                new Log { TipAkcije = "PRIJAVA", Nivo = NivoLoga.INFO, VrijemeAkcije = DateTime.UtcNow.AddHours(-3), KorisnikId = mistralId != 0 ? mistralId : null, IpAdresa = "10.0.0.5", Detalji = "Uspjesna prijava: firma@etf.ba" }
            );

            await context.SaveChangesAsync();
        }
    }
}
