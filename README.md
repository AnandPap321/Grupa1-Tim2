# ETF Talent Program

## Uputstvo za razvoj aplikacije

1. Kako bi se ostvarila ispravna konekcija na bazu podataka kroz "DefaultConnection" ConnectionString definisan u Program.cs file-u, potrebno ga je postaviti lokalno koristeci sljedece komande:

   > dotnet user-secrets init

   > dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"

   U posljedjoj komandi zamijenite your-connection-string sa vasim stringom za konekciju na bazu.
