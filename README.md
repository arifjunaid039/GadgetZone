# The Gadget Zone Official

Academic ASP.NET Core MVC store built with a layered architecture:

- **Presentation:** `TheGadgetZone.Web` (MVC controllers, Razor views, and view models)
- **Business:** `TheGadgetZone.Business` (entities, store rules, and repository contract)
- **Data Access:** `TheGadgetZone.DataAccess` (EF Core, SQL Server, Identity, and repository implementation)

## Requirements

- .NET 10 SDK
- SQL Server LocalDB (or a SQL Server connection string)

## Run locally

Open PowerShell at the repository root, `E:\AI Native Engineering\Class 9`, and run:

```powershell
dotnet restore
dotnet build TheGadgetZoneOfficial.slnx
```

The default connection string targets `(localdb)\MSSQLLocalDB`. On first startup, the app applies EF Core migrations, seeds the eight categories, and creates the Customer and Admin roles.

To provision an initial administrator, configure local user secrets before running the app. Do not commit the password:

```powershell
dotnet user-secrets init --project src\TheGadgetZone.Web\TheGadgetZone.Web.csproj
dotnet user-secrets set "InitialAdmin:Email" "admin@example.com" --project src\TheGadgetZone.Web\TheGadgetZone.Web.csproj
dotnet user-secrets set "InitialAdmin:Password" "Choose-A-Strong-Local-Password1!" --project src\TheGadgetZone.Web\TheGadgetZone.Web.csproj
```

Start the site from the repository root:

```powershell
dotnet run --project .\src\TheGadgetZone.Web\TheGadgetZone.Web.csproj --urls http://localhost:5080
```

Open `http://localhost:5080`. Without the two Admin settings, the app still seeds categories and roles, but no administrator account is created. Customers can register through the site.

If PowerShell is already inside `src\TheGadgetZone.Web`, run `dotnet run --urls http://localhost:5080` instead. Do not use the root-relative `.\src\...` project path from inside that project folder.

### Windows Application Control

If startup fails with `FileLoadException` and `An Application Control policy has blocked this file`, the Windows Code Integrity policy is rejecting the locally built Web assembly. This is an operating-system signing/allow-list decision, not a missing-project-path or MVC error; changing the `dotnet run` path will not resolve it. Ask the device administrator/IT team to approve the development build or provide an authorized development environment. Do not disable or work around the device's application-control policy.

To apply or inspect migrations manually:

```powershell
dotnet ef database update --project src\TheGadgetZone.DataAccess\TheGadgetZone.DataAccess.csproj --startup-project src\TheGadgetZone.Web\TheGadgetZone.Web.csproj
```

Run the business tests with:

```powershell
dotnet test TheGadgetZoneOfficial.slnx
```

## Included assignment scope

Customer registration/login, product browsing/search, category filtering, cart, COD checkout, order history, role-protected Admin catalogue/stock/order management, Contact Us map, and the required store footer.
