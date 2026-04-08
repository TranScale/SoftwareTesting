# PHASE 0 - PROJECT AUDIT

## Submission Context

- Submission title: `Powered by GPT - Software Quality Verification`
- Course: `Software Testing`
- Project under test: `Online Sales Management System`
- Repository type: `ASP.NET Core MVC (.NET 8) + EF Core + ASP.NET Identity + TiDB/MySQL`
- Audit date: `2026-04-05`

## Objective

This audit establishes the real testing scope for the final submission by using the live exam brief, the actual source code, seeded data, and a local smoke run. The goal is to identify the most testable business flows, role-based boundaries, API scope, data availability, and the highest-value areas for manual and automation testing.

## What Was Analyzed

### 1. Exam brief and rubric

- `Report Test subject/UEF - Final.pdf`
- Required deliverables confirmed:
  - Main: `Word report`, `PPTX slides`
  - Shared link: `GitHub` or `Google Drive`
  - Linked artifacts: `Test Cases`, `Test Script + Test Data`, `Final Results + Images`, `Automation Video`
- Mandatory rule confirmed: `3 members => minimum 30 non-overlapping executed test cases`

### 2. Real project structure

- Startup and routing: `Program.cs`
- Data layer and schema: `Data/ApplicationDbContext.cs`
- Seed data and seeded roles/users: `Data/DbSeeder.cs`
- Public storefront: `Controllers/HomeController.cs`, `Controllers/ProductController.cs`
- API surface: `Controllers/Api/HealthController.cs`, `Controllers/Api/CatalogController.cs`
- Admin flows: controllers under `Areas/Admin/Controllers`
- Views for testable UI screens: `Areas/Admin/Views/*`, `Views/*`

### 3. Local verification completed

- `dotnet build` completed successfully with `0 errors` and `0 warnings`
- Local smoke run succeeded using user secrets for TiDB
- Verified real responses:
  - `GET /api/v1/health` -> `200 OK`
  - `GET /api/v1/catalog/products?page=1&pageSize=3` -> `200 OK`
  - `GET /Admin/Auth/Login` -> `200 OK`
- Verified seeded login accounts work in practice:
  - `admin@osms.local`
  - `sales@osms.local`
  - `warehouse@osms.local`
- Verified role access behavior in practice:
  - `admin` can access dashboard, invoices, purchases, admins
  - `sales` can access dashboard, invoices, reports but is denied for purchases and admins
  - `warehouse` can access dashboard, products, purchases but is denied for invoices and reports

## Assumptions

- The TiDB database used during audit is the intended demo environment for the final submission.
- Seeded accounts and demo data are acceptable for test execution and report evidence unless the team later resets the database.
- The final submission will use `GitHub Issues` for bug management because the repository is already Git-based and the rubric accepts GitHub Issues.
- The team wants maximum-score coverage, so the test scope should go beyond the minimum 30 cases and include both positive and negative coverage.

## Risks

- The current database is environment-dependent because it relies on `User Secrets` for the TiDB password.
- Any later reseeding may change entity IDs, stock values, invoice totals, and execution screenshots.
- The project has both a public storefront and an admin portal, but the root route currently redirects to admin login, which may affect public UI scope and navigation evidence.
- The available API surface is read-focused; there are no obvious admin write APIs to support broad authenticated API coverage.

## Acceptance Criteria For This Audit

- The audit must identify the real business modules and testable flows from source code.
- The audit must separate `confirmed testable now` from `needs later real execution`.
- The audit must nominate `3-5 Selenium flows` and `3-5 API test groups` based on the actual project.
- The audit must produce a proposed artifact structure that later phases can fill without rework.

## Project Overview

`Online Sales Management System` is a role-based web application with two visible surfaces:

1. A `public storefront` for browsing products, filters, product details, and trending categories/products.
2. An `admin portal` for authentication, dashboard analytics, master data management, inventory, sales, reporting, settings, and role/permission administration.

The system uses:

- `ASP.NET Core MVC` for server-rendered UI
- `ASP.NET Identity` for login and account management
- `Permission-based authorization` for module-level access control
- `EF Core + Pomelo MySQL provider` against `TiDB`
- `ClosedXML` for Excel import/export features

The application contains meaningful business logic for:

- stock increase on `purchase receive`
- stock decrease on `invoice create`
- stock return on `invoice cancel`
- payment status calculation
- low-stock thresholds
- permission restrictions by role/group
- data import/export via Excel

This is sufficient scope for a strong final testing submission.

## Module Inventory

| Module | Real Location | UI Testable | API Testable | Priority | Notes |
|---|---|---:|---:|---|---|
| Authentication | `Areas/Admin/Controllers/AuthController.cs` | Yes | No | Critical | Login/logout/access denied |
| Dashboard | `Areas/Admin/Controllers/DashboardController.cs` | Yes | No | High | Role-specific landing page |
| Admin Users | `Areas/Admin/Controllers/AdminsController.cs` | Yes | No | High | Create/edit/disable/delete admin accounts |
| Admin Groups & Permissions | `Areas/Admin/Controllers/AdminGroupsController.cs` | Yes | No | Critical | Permission matrix, grant-all, protected super admin |
| Customers | `Areas/Admin/Controllers/CustomersController.cs` | Yes | Partial | High | CRUD, details, quick info JSON |
| Suppliers | `Areas/Admin/Controllers/SuppliersController.cs` | Yes | No | Medium | CRUD and purchase history |
| Employees | `Areas/Admin/Controllers/EmployeesController.cs` | Yes | No | Medium | CRUD |
| Attendance | `Areas/Admin/Controllers/AttendanceController.cs` | Yes | No | Medium | Daily marking workflow |
| Categories | `Areas/Admin/Controllers/CategoriesController.cs` | Yes | No | Medium | CRUD + trending toggle |
| Brands | `Areas/Admin/Controllers/BrandsController.cs` | Yes | No | Medium | CRUD |
| Units | `Areas/Admin/Controllers/UnitsController.cs` | Yes | No | Medium | CRUD |
| Products | `Areas/Admin/Controllers/ProductsController.cs`, `ProductWriteController.cs` | Yes | Partial | Critical | CRUD, image upload, Excel import/export, trending toggle |
| Purchases | `Areas/Admin/Controllers/PurchasesController.cs` | Yes | No | Critical | Draft -> received/cancel lifecycle, stock in |
| Invoices | `Areas/Admin/Controllers/InvoicesController.cs` | Yes | No | Critical | Create, details, print, record payment, cancel, stock out |
| Expenses | `Areas/Admin/Controllers/ExpensesController.cs` | Yes | No | Medium | CRUD |
| Stock | `Areas/Admin/Controllers/StockController.cs` | Yes | No | Critical | stock list, low stock, movement history, export |
| Reports | `Areas/Admin/Controllers/ReportsController.cs` | Yes | No | High | Date filters, totals, Excel export |
| Settings | `Areas/Admin/Controllers/SettingsController.cs` | Yes | No | Low | Company info/logo |
| Public Home | `Controllers/HomeController.cs` | Yes | No | Medium | Trending categories and products |
| Public Product Catalog | `Controllers/ProductController.cs` | Yes | Mirrors API | High | Search, filter, sort, pagination, details |
| Health API | `Controllers/Api/HealthController.cs` | No | Yes | Low | Smoke/availability |
| Catalog API | `Controllers/Api/CatalogController.cs` | No | Yes | High | Product list/detail/trending/filters |

## Real Data Availability

### Seeded accounts confirmed

- `admin@osms.local / Admin@12345`
- `sales@osms.local / Sales@12345`
- `warehouse@osms.local / Warehouse@12345`

### Seeded business data inferred from source

- `6` suppliers
- `15` customers
- `6` employees
- `5` categories
- `15` brands
- `7` units
- `36` seeded products, with inactive records possible
- `24` purchases with mixed statuses
- `60` invoices with mixed statuses
- `attendance` for the last 30 days
- `expenses`, `stock movements`, and `settings`

### Runtime observation

- Catalog API currently exposes `34 active products`, which aligns with seeded active/inactive logic.

## Proposed Test Scope

### In-scope for final submission

- Admin authentication and session behavior
- Role-based access control for `admin`, `sales`, and `warehouse`
- Master data CRUD with validation:
  - customers
  - suppliers
  - products
  - brands
  - categories
  - units
- Core business workflows:
  - create purchase
  - receive purchase
  - cancel purchase
  - create invoice
  - record payment
  - cancel invoice
  - verify stock movement impact
- Inventory views:
  - stock list
  - low-stock list
  - stock movement history
- Reports and Excel export
- Public catalog:
  - search
  - category filter
  - brand filter
  - price filter
  - sort
  - product details
- API testing for:
  - health
  - catalog list
  - catalog detail
  - trending
  - filters

### Out-of-scope or limited-scope

- Third-party assets loaded from CDN:
  - Google Fonts
  - Tailwind CDN
  - external placeholder images
- Deep performance/load testing
- Mobile app testing
- Browser/device matrix beyond a focused cross-browser set
- Security penetration testing beyond access control and input validation
- Password reset flow, because the UI shows a link but no completed recovery workflow is evident yet
- Admin API write operations, because a broad authenticated admin API surface is not implemented

## Testability Assessment

### Confirmed testable now

- Build and startup
- Login with all 3 seeded accounts
- Public catalog API
- Admin permission boundaries
- Admin and public server-rendered pages
- Excel import/export and export endpoints by code path

### Strongly testable from current implementation

- Input validation on CRUD forms
- Unauthorized/forbidden navigation
- Business status transitions in purchases and invoices
- Stock consistency after purchase/invoice actions
- Search/filter/pagination behavior
- Excel upload preview and confirm flow for products

### Current blockers for complete execution evidence

- `PENDING REAL EXECUTION`: full manual execution across all target browsers
- `PENDING REAL EXECUTION`: screenshot evidence for each final result row
- `PENDING REAL EXECUTION`: confirmed defects with attachments and GitHub Issue links
- `PENDING REAL EXECUTION`: automation video capture

### Technical blockers or dependencies

- DB connectivity depends on `User Secrets`
- Some evidence will change if demo data is reseeded
- Public storefront landing behavior is affected by root-route redirect

## Business Risks And Edge-Case Hotspots

### Highest-risk business areas

1. `Invoice lifecycle`
   - payment amount normalization
   - unpaid/partial/paid status transitions
   - stock deduction at creation
   - stock return on cancellation
2. `Purchase lifecycle`
   - draft vs received vs cancelled behavior
   - stock increment only on receive
3. `Role-based permissions`
   - sales vs warehouse vs admin access
   - protected super admin account/group actions
4. `Product import/export`
   - duplicate SKU
   - invalid category/unit/brand references
   - expired preview cache
   - malformed numeric data
5. `Reports and stock visibility`
   - date-range correctness
   - cancelled record exclusion
   - report totals vs source transactions

### Best areas to hunt for real bugs

These are not claimed as confirmed defects yet. They are the most promising areas for later real execution:

1. `Invoice cancellation on non-standard statuses`
   - Source code allows cancellation unless status is already `Cancelled`.
   - Seeded data includes `Draft` invoices.
   - This makes draft-invoice stock restoration a likely defect candidate and must be executed before logging.
2. `Public storefront routing`
   - The project contains a public `HomeController`, but `/` is explicitly redirected to `/Admin/Auth/Login`.
   - This may create a real UX/navigation defect for storefront access and breadcrumb behavior.
3. `Public catalog sorting UI`
   - `Views/Product/Index.cshtml` contains malformed `<!option ...>` markup in the sorting dropdown.
   - This is a strong candidate for a real UI rendering defect and needs browser confirmation.
4. `Report date and timezone handling`
   - Some admin flows store UTC while forms display Vietnam-local dates.
   - Boundary-date verification is required around reports, stock movements, and invoice timestamps.
5. `Repeated state transitions`
   - Purchase cancel/receive and invoice pay/cancel should be executed repeatedly to verify duplicate-action protection.

## Proposed Testing Strategy

### Overall approach

Use a mixed `manual + automation` strategy, with `black-box testing` for final deliverables and `white-box-informed test design` based on source-code inspection. This gives strong rubric coverage while staying honest about what has and has not been executed.

### Recommended scope split

- `Manual UI testing`
  - primary coverage for CRUD, validation, permissions, reports, and negative scenarios
- `API testing`
  - focused on read APIs and validation logic in `CatalogController`
- `Automation`
  - small, high-value regression pack only
  - prioritize stable business-critical flows over broad but shallow automation

### Target test case volume

- Mandatory minimum: `30 executed non-overlapping cases`
- Recommended scoring target: `36-45 total cases`
  - `12-15` for Hoàng Văn Thiên
  - `10-12` for Nguyễn Thành Đạt
  - `10-12` for Lê Quang Duy

### Test design principle

- Every high-priority module must include:
  - positive flow
  - negative validation
  - permission or authorization check
  - edge case or business rule check

## Best Selenium Candidates (3-5)

1. `Admin login and role-based access smoke`
   - Login with `admin`, `sales`, `warehouse`
   - Verify allowed and denied modules
   - High rubric value because it proves permissions and role separation
2. `Create invoice and verify resulting status`
   - Select customer
   - Add products
   - Enter payment variants
   - Verify expected status (`Unpaid`, `PartiallyPaid`, or `Paid`)
3. `Create purchase then receive purchase`
   - Create draft purchase
   - Receive it
   - Verify stock update and movement history
4. `Product Excel import preview/confirm`
   - Upload valid/invalid rows
   - Verify preview counts and import result
5. `Public product catalog filters`
   - Search + category + brand + price + detail
   - Suitable if UI selectors are stable enough after execution check

## Best API Test Groups (3-5)

1. `Health API`
   - `GET /api/v1/health`
   - smoke and availability baseline
2. `Catalog product list API`
   - `GET /api/v1/catalog/products`
   - pagination, sort, search, filter, validation errors
3. `Catalog product detail API`
   - `GET /api/v1/catalog/products/{id}`
   - valid id, invalid id, not found, inactive product
4. `Trending API`
   - `GET /api/v1/catalog/trending`
   - category/product trending payload
5. `Filters API`
   - `GET /api/v1/catalog/filters`
   - active category/brand counts and schema checks

## Proposed Deliverable Repo Structure

This is the draft structure for the testing submission package. It is intentionally aligned with the exam brief and will be finalized in Phase 1.

```text
testing-submission/
  README.md
  report/
    Powered by GPT - Software Quality Verification.docx
    Powered by GPT - Software Quality Verification.pdf
  slides/
    Powered by GPT - Software Quality Verification.pptx
  docs/
    phase-0-project-audit.md
    test-plan.md
    test-scenarios.md
    defect-workflow.md
  test-cases/
    ui-test-cases.xlsx
    api-test-cases.xlsx
  test-data/
    ui-test-data.xlsx
    api-test-data.json
    test-accounts.md
  automation/
    ui/
    api/
    README.md
  defects/
    defect-log.xlsx
    github-issues/
  evidence/
    ui/
    api/
    defects/
  results/
    final-test-results.xlsx
    metrics-summary.xlsx
  video/
    automation-demo.mp4
```

## Audit Conclusion

The project is large enough for a maximum-score final testing submission if the team stays disciplined about scope and evidence. The strongest final package should focus on:

- `permission testing`
- `invoice/purchase/stock business rules`
- `catalog and report validation`
- `Excel import/export`
- `lightweight but real automation`

The project does have genuine API scope, but it is narrower than the UI scope. Therefore, the submission should treat `UI testing as the main body` and `API testing as focused supporting coverage`.

## Missing Real Data That Still Blocks Later Phases

- `PENDING REAL EXECUTION`: final screenshots for each executed case
- `PENDING REAL EXECUTION`: confirmed defect evidence before defect log authoring
- `PENDING REAL EXECUTION`: final environment matrix for actual execution browsers
- `PENDING REAL EXECUTION`: automation video recording
- `PENDING REAL EXECUTION`: final test ownership allocation by exact test case IDs
