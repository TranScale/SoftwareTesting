# Source To Testing Mapping

## Objective

Map the real source project to the testing submission structure so every planned artifact has a clear technical origin.

## Mapping Table

| Business area | Source files | Planned testing artifacts | Notes |
|---|---|---|---|
| Authentication | `Areas/Admin/Controllers/AuthController.cs`, `Areas/Admin/Views/Auth/Login.cshtml` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `evidence/ui/`, `results/OSMS-Final-Test-Results.xlsx` | login, invalid login, inactive account, logout, access denied |
| Admin users | `Areas/Admin/Controllers/AdminsController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `defects/exports/OSMS-Defect-Log.xlsx` | create, edit, disable, self-protection, super admin protection |
| Admin groups and permissions | `Areas/Admin/Controllers/AdminGroupsController.cs`, `Services/Security/*` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `evidence/ui/`, `defects/` | permission matrix and forbidden access are high-value scoring areas |
| Customers | `Areas/Admin/Controllers/CustomersController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `test-data/ui/OSMS-UI-Test-Data.xlsx` | CRUD, quick info, detail and invoice history |
| Suppliers | `Areas/Admin/Controllers/SuppliersController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx` | CRUD and supplier detail history |
| Products | `Areas/Admin/Controllers/ProductsController.cs`, `Areas/Admin/Controllers/ProductWriteController.cs`, `Areas/Admin/Views/Products/*` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `test-data/ui/OSMS-UI-Test-Data.xlsx`, `automation/ui/`, `evidence/ui/` | product CRUD, image upload, Excel preview, import confirm, export |
| Purchases | `Areas/Admin/Controllers/PurchasesController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `results/OSMS-Final-Test-Results.xlsx`, `automation/ui/` | create, receive, cancel, stock-in verification |
| Invoices | `Areas/Admin/Controllers/InvoicesController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `results/OSMS-Final-Test-Results.xlsx`, `automation/ui/`, `defects/` | create, payment, cancel, stock-out, print |
| Stock | `Areas/Admin/Controllers/StockController.cs`, `Services/Inventory/StockService.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `evidence/reports/`, `results/OSMS-Final-Test-Results.xlsx` | low stock, movement history, export |
| Reports | `Areas/Admin/Controllers/ReportsController.cs` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `metrics/OSMS-Test-Metrics.xlsx`, `evidence/reports/` | date filter, totals, export correctness |
| Public catalog UI | `Controllers/HomeController.cs`, `Controllers/ProductController.cs`, `Views/Home/Index.cshtml`, `Views/Product/*` | `test-cases/ui/OSMS-UI-Test-Cases.xlsx`, `automation/ui/`, `evidence/ui/` | search, filter, sort, details, trending |
| Health API | `Controllers/Api/HealthController.cs` | `test-cases/api/OSMS-API-Test-Cases.xlsx`, `automation/api/`, `evidence/api/` | smoke and service availability |
| Catalog API | `Controllers/Api/CatalogController.cs` | `test-cases/api/OSMS-API-Test-Cases.xlsx`, `test-data/api/OSMS-API-Test-Data.json`, `automation/api/`, `evidence/api/` | pagination, validation, sort, filter, detail, trending, filters |
| Seeded demo data | `Data/DbSeeder.cs` | `test-data/accounts/OSMS-Test-Accounts.md`, `test-data/ui/OSMS-UI-Test-Data.xlsx`, `test-data/api/OSMS-API-Test-Data.json` | source of seeded accounts, roles, and demo entities |

## Traceability Rule

Every major module tested in the report should have this chain:

`source file -> test case -> test data -> execution result -> evidence -> defect or metric`

If any link in that chain is missing, the report will be weaker during grading or defense.

## Priority Coverage Order

Build the later phases around this order:

1. Authentication and permissions
2. Purchases, invoices, and stock
3. Products and product import/export
4. Reports
5. Public catalog UI and catalog API

## Known Evidence Risks

- The public storefront exists in source, but `/` currently redirects to admin login.
- The catalog sorting UI contains malformed option markup and must be browser-verified before it is logged as a defect.
- Invoice cancellation behavior on non-standard statuses must be executed before any defect claim is written.
