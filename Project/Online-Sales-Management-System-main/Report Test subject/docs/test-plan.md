# TEST PLAN

## 1. Project Information

- Submission title: `Powered by GPT - Software Quality Verification`
- Course: `Software Testing`
- System under test: `Online Sales Management System`
- Repository URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System`
- Appendix folder URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject`
- Application architecture: `ASP.NET Core MVC (.NET 8) + EF Core + ASP.NET Identity + TiDB/MySQL`
- Main test surfaces:
  - `Admin Portal`
  - `Public Product Catalog`
  - `REST API: /api/v1/health and /api/v1/catalog/*`

## 2. Objective

This test plan defines the real testing strategy for the `Online Sales Management System` final submission. The plan is based on the live exam brief, source-code inspection, seeded demo data, and a local smoke run completed on `2026-04-05`.

The main objective is to verify that the system supports the expected operational workflows of an online sales management platform, with emphasis on:

- secure admin authentication
- correct permission boundaries between roles
- correct sales and purchasing behavior
- correct inventory updates
- stable product and master-data management
- correct public catalog browsing and filtering
- valid API responses for the exposed catalog endpoints

## 3. Test Objectives

The testing effort will aim to:

1. Confirm the correctness of critical business flows in `Purchases`, `Invoices`, `Stock`, and `Reports`.
2. Verify that `admin`, `sales`, and `warehouse` accounts only access the modules permitted by their assigned permission groups.
3. Validate CRUD and business-rule behavior in the main admin modules:
   - customers
   - suppliers
   - products
   - brands
   - categories
   - units
4. Validate public catalog behavior for search, filtering, sorting, pagination, and product details.
5. Validate API input handling and response structures for the exposed `catalog` and `health` endpoints.
6. Produce evidence and traceability strong enough for the final report, appendix, and GitHub submission.

## 4. Scope

### 4.1 In Scope

- Admin login, logout, invalid login, inactive-account handling, and access-denied behavior
- Role-based access control for:
  - `admin@osms.local`
  - `sales@osms.local`
  - `warehouse@osms.local`
- Admin user and admin group management
- Permission matrix behavior
- Customer management
- Supplier management
- Product management, including:
  - create
  - edit
  - disable
  - trending toggle
  - Excel import preview
  - Excel import confirm
  - Excel export
- Purchase workflow:
  - create draft purchase
  - receive purchase
  - cancel purchase
- Invoice workflow:
  - create invoice
  - server-side product pricing usage
  - payment recording
  - invoice cancellation
  - stock deduction and return behavior
- Inventory and stock visibility:
  - stock list
  - low stock list
  - stock movement history
  - stock movement export
- Reports:
  - date filter behavior
  - sales total
  - purchase total
  - profit calculation
  - report export
- Public catalog:
  - trending content
  - product list
  - search
  - category filter
  - brand filter
  - price filter
  - sorting
  - pagination
  - product details
- API testing:
  - `GET /api/v1/health`
  - `GET /api/v1/catalog/products`
  - `GET /api/v1/catalog/products/{id}`
  - `GET /api/v1/catalog/trending`
  - `GET /api/v1/catalog/filters`

### 4.2 Out Of Scope

- Mobile app testing
- Performance, stress, and load testing
- Full penetration testing
- Third-party CDN availability issues for Google Fonts or external image hosts
- Password recovery workflow, because the UI contains a placeholder link but no confirmed full recovery implementation
- Admin write APIs beyond the exposed catalog and health endpoints
- Multi-device responsive certification beyond desktop-browser verification

## 5. Test Strategy And Approach

### 5.1 Overall Approach

This project will use a mixed `manual + automation` strategy.

- `Manual testing` will be the primary approach for broad UI coverage, business-rule verification, validation messages, permission checks, and defect discovery.
- `API testing` will be used to verify endpoint behavior, response structure, validation logic, and data consistency for the exposed API surface.
- `Automation testing` will be applied selectively to a small, high-value regression pack in order to maximize scoring without overextending scope.

The team will use a `black-box execution approach` during test execution while using `white-box-informed test design` from source-code analysis to identify hidden edge cases, risky branches, and permission rules.

### 5.2 Manual Vs Automation

#### Manual testing will cover

- exploratory validation around forms and business rules
- negative scenarios
- role-based access restrictions
- report and export verification
- defect confirmation and evidence capture
- visual rendering and user-facing messages

#### Automation testing is planned for

- admin login and role-based smoke flow
- invoice creation with status validation
- purchase creation and receiving flow
- product Excel import preview and confirm
- catalog API request validation and smoke coverage

`PENDING REAL EXECUTION`: automation framework implementation and evidence belong to later phases and are not yet present in the repository.

### 5.3 Black-Box And White-Box Usage

#### Black-box testing

- main method for UI and API execution
- validates expected behavior from the end-user perspective
- supports report-friendly reproducible steps

#### White-box-informed test design

- used to derive high-risk scenarios from the source code
- used to design edge cases for:
  - invoice status transitions
  - stock updates
  - permission enforcement
  - API validation rules
  - Excel import rules

There is currently no dedicated automated unit-test project in the repository. Therefore, white-box use in this submission focuses on `risk-based test design`, not code-level assertion suites.

## 6. Test Items By Priority

### Critical Priority

- Authentication
- Role and permission control
- Purchases
- Invoices
- Stock
- Products

### High Priority

- Reports
- Admin users
- Admin groups
- Public product catalog
- Catalog API

### Medium Priority

- Customers
- Suppliers
- Categories
- Brands
- Units
- Employees
- Attendance
- Expenses

### Low Priority

- Settings
- Cosmetic UI behaviors not affecting business outcomes

## 7. Test Design Principles

The test cases in later phases will follow these principles:

- every critical module must include both positive and negative coverage
- every important form must include validation and boundary checks
- permission-sensitive modules must include unauthorized access attempts
- high-risk business modules must include repeat-action and edge-condition coverage
- API cases must include validation error handling, not only `200 OK` scenarios
- test steps must be reproducible and evidence-friendly

## 8. Test Environment

### 8.1 Verified Environment

- OS: `Microsoft Windows NT 10.0.26100.0`
- Repository working directory: `E:\Project\Online-Sales-Management-System`
- Runtime: `.NET 8`
- Database: `TiDB/MySQL` through EF Core and Pomelo provider
- Browser confirmed on machine: `Google Chrome`
- Shell and execution tools confirmed on machine:
  - `PowerShell`
  - `dotnet`
  - `git`
  - `node`
  - `npm`
  - `npx`
  - `java`

### 8.2 Verified Application Endpoints

- Local HTTP profile: `http://localhost:5068`
- Local HTTPS profile from launch settings: `https://localhost:7248`
- Admin login page: `/Admin/Auth/Login`
- Health API: `/api/v1/health`
- Catalog API root: `/api/v1/catalog`

### 8.3 Data Environment

The current environment uses seeded demo data from `Data/DbSeeder.cs`, including:

- seeded admin accounts
- customers
- suppliers
- products
- purchases
- invoices
- stock movements
- expenses
- attendance records

### 8.4 Browser Matrix

- Primary browser for execution: `Google Chrome`
- Secondary browser for bonus cross-browser scope: `NEEDS USER CONFIRMATION`
- Tertiary browser for bonus cross-browser scope: `NEEDS USER CONFIRMATION`

## 9. Test Tools

### 9.1 Confirmed Tools

- `GitHub` for repository traceability and planned defect management
- `Git` for versioning
- `dotnet build` and local run profiles for application verification
- `PowerShell` for local execution support
- `Google Chrome` for browser-based UI execution

### 9.2 Planned Tools For Later Phases

- `GitHub Issues` for defect logging with severity and priority screenshots
- `Selenium` for UI automation
- `Postman` or `Newman` for API testing and repeatable execution
- `Excel` workbooks for test cases, test data, results, and metrics

`NEEDS USER CONFIRMATION`: exact automation toolchain selection between raw Selenium WebDriver, Selenium IDE, or another browser automation runner in later phases.

## 10. Test Data Strategy

The test data strategy will combine:

- seeded system accounts
- seeded business records from the database
- reusable valid inputs
- invalid and malformed inputs
- duplicate and boundary-value data
- API query combinations

Sensitive values such as live database secrets must not be committed. User Secrets remain environment-only and are out of the GitHub submission package.

## 11. Entry Criteria

Testing execution may begin when the following conditions are met:

1. The repository builds successfully.
2. The application starts locally and the login page is reachable.
3. The TiDB connection works with the configured User Secrets.
4. Seeded accounts can authenticate successfully.
5. The required test-case and test-data workbooks are prepared for the planned execution scope.
6. The target modules and test ownership are finalized for the three team members.

## 12. Exit Criteria

The planned cycle will be considered complete when:

1. At least `30` non-overlapping test cases are designed and executed.
2. Each member completes the minimum assigned case volume:
   - Hoang Van Thien: `12`
   - Nguyen Thanh Dat: `10`
   - Le Quang Duy: `10`
3. UI and API test results are recorded in the final results workbook.
4. Defects confirmed during execution are logged in `GitHub Issues` with severity, priority, steps, and screenshots.
5. Required screenshots and execution evidence are captured and stored in the `evidence/` structure.
6. Final metrics and summary tables are ready for the report and slides.
7. Automation evidence is either provided or clearly marked `PENDING REAL EXECUTION`.

## 13. Defect Management Plan

The defect management process for this submission will use `GitHub Issues`.

Each confirmed defect must include:

- defect ID or issue link
- module
- title
- environment
- precondition
- steps to reproduce
- expected result
- actual result
- severity
- priority
- attachment screenshots
- current status

Defects must not be invented. Any suspected issue identified from source review must remain unlogged until execution confirms it.

## 14. Team Allocation

### 14.1 Planned Ownership

- `Hoang Van Thien`:
  - authentication and role access
  - admin users and permissions
  - invoices
  - reports
  - target volume: `12 test cases`
- `Nguyen Thanh Dat`:
  - customers
  - suppliers
  - purchases
  - expenses
  - target volume: `10 test cases`
- `Le Quang Duy`:
  - products
  - stock
  - public catalog
  - exposed API coverage
  - target volume: `10 test cases`

### 14.2 Allocation Notes

- The above allocation is intentionally non-overlapping at module level.
- Hoang Van Thien carries slightly more workload to reflect the requested team balance.
- Final test-case IDs and exact member ownership will be locked in later case-design phases.

## 15. Risks And Mitigation

| Risk | Impact | Mitigation |
|---|---|---|
| TiDB password is stored in User Secrets, not in the repo | execution may fail on another machine | document setup, verify login and health endpoints before formal execution |
| Demo data may change after reseeding | screenshots and expected values may become inconsistent | freeze the database state before final execution evidence collection |
| Root route redirects to admin login | public storefront coverage may be confusing in the report | test public pages using direct routes such as `/Product` and document the routing behavior clearly |
| Permission behavior may differ by role unexpectedly | wasted execution time and false assumptions | run role smoke checks first before full module execution |
| Invoice and purchase workflows change stock values | later tests may be affected by earlier tests | use controlled sequencing and record the before/after stock state for business-critical executions |
| Browser coverage may be limited to Chrome only | weaker bonus coverage | use Chrome as baseline and add a second browser only if environment is confirmed available |
| No automation framework exists yet in the repo | automation scope may expand too late | keep automation small and focused on 3-5 stable flows only |
| Evidence may become disorganized | weak traceability in the final report | follow the naming convention and file-placement rules from Phase 1 |

## 16. Deliverables Planned From This Test Plan

This test plan feeds the following final artifacts:

- `docs/test-plan.md`
- `docs/test-scenarios.md`
- `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
- `test-cases/api/OSMS-API-Test-Cases.xlsx`
- `test-data/ui/OSMS-UI-Test-Data.xlsx`
- `test-data/api/OSMS-API-Test-Data.json`
- `results/OSMS-Final-Test-Results.xlsx`
- `metrics/OSMS-Test-Metrics.xlsx`
- `defects/exports/OSMS-Defect-Log.xlsx`
- `automation/*`
- `evidence/*`
- `video/OSMS-Automation-Demo.mp4`

## 17. Items Requiring Confirmation

- `NEEDS USER CONFIRMATION`: which second browser, if any, will be used for cross-browser execution
- `NEEDS USER CONFIRMATION`: exact automation stack to be implemented in Phase 8
- `PENDING REAL EXECUTION`: final screenshots, final execution statuses, and final defect counts

## 18. Current Readiness Statement

The project is ready to move from planning into scenario design. The real system has enough business scope, seeded data, and verified local operability to support a strong final testing submission, provided that later phases keep execution evidence honest and traceable.
