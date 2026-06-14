# ETF Talent Program

## O projektu

ETF Talent Program je sistem namijenjen Elektrotehnickom fakultetu u Sarajevu s ciljem uvezivanja studenata sa potencijalnim poslodavcima. Ovaj sistem studentima omogucava prezentovanje vlastitih vjestina i priliku za prvi angazman na stvarnim projektima, a firmama pronalazak talenata i olaksan proces regrutacije.

## Live verzija aplikacije

https://grupa1-tim2.onrender.com

## Uputstvo za razvoj aplikacije

1. *Konekcija na remote bazu podataka*

   Kako bi se ostvarila ispravna konekcija na remote bazu podataka kroz "DefaultConnection" ConnectionString definisan u Program.cs file-u, potrebno ga je postaviti lokalno koristeci sljedece komande:

   `dotnet user-secrets init`

   `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"`

   U posljedjoj komandi zamijenite your-connection-string sa vasim stringom za konekciju na bazu.

   **Alternativno**, ako se ne postavi konekcijski string na remote bazu, aplikacija ce koristiti lokalnu bazu onako kako je definisano u application.json file-u. 

2. *Update baze podataka nakon nove migracije*

   Ako je neki clan tima postavio novu migraciju (verziju baze), nakon povlacenja izmjena (git pull) potrebno je uraditi sljedecu komandu kako bi lokalna baza imala isti ocekivani izgled kao sto je naznaceno u ApplicationDbContextModelSnapshot.cs file-u:

   `dotnet ef database update`

   **Alternativno**, moguce je jednostavno pokrenuti aplikaciju nakon cega ce `DbInitializer.cs` izvrsiti automatski update baze uz pomoc linije koda koja je zapisana u njemu: `await context.Database.MigrateAsync();`

## Stek tehnologija koje se koriste na projektu

### Backend
- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core

### Frontend
- Razor Views
- Bootstrap 5
- JavaScript

### Database
- SQL Server

### Authentication & Authorization
- ASP.NET Core Identity

### Infrastructure & Deployment
- Docker - kontejnerizacija
- Render - deployment frontenda + backenda
- SmarterASP.NET - deployment baze

### Development Tools
- Visual Studio 2026
- SQL Server Management Studio 22.7.0
