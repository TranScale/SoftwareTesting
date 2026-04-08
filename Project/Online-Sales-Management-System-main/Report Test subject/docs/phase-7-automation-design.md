# Phase 7 Automation Design

## Objective

Select the smallest automation scope that still maximizes rubric score, bonus potential, and real execution feasibility for the current OSMS project.

## Real feasibility findings

- The application is an ASP.NET Core MVC project, so a `.NET 8` UI automation project fits the main stack and local toolchain.
- Verified local tools: `dotnet`, `node`, `npm`, `npx`, `java`.
- `mvn` is not installed, so Java Selenium would add unnecessary setup friction.
- `Google Chrome` is installed locally.
- `chromedriver` and `msedgedriver` are not preinstalled, so the design should rely on Selenium Manager rather than manual driver binaries.
- Current public API surface is small and stable, which makes `Postman + Newman` a practical API automation choice.

## Automation stack decision

### UI automation

- Framework: `.NET 8 + xUnit + Selenium WebDriver`
- Browser management: `Selenium Manager`
- Pattern: `Page Object Model`
- Evidence mode: `headed Chrome` for screenshots and demo video
- Re-run mode: optional headless Chrome after evidence capture

### API automation

- Framework: `Postman Collection + Environment + Newman`
- Assertion style: built-in Postman test scripts
- Execution mode: Newman CLI for repeatable regression runs

## Priority UI automation scope

These flows were selected because they are business-critical, reproducible from seeded data, and give better scoring value than purely cosmetic checks.

| Auto ID | Related Test Case IDs | Flow | Why this should be automated | Data plan | State risk |
|---|---|---|---|---|---|
| `AUTO-UI-001` | `TC-UI-AUTH-001` | Admin login smoke | Foundation flow for every admin test; fast to rerun and easy to evidence | `admin@osms.local / Admin@12345` | Low |
| `AUTO-UI-002` | `TC-UI-AUTH-003` | Sales user is denied access to Purchases | High-value negative permission case that shows access control coverage | `sales@osms.local / Sales@12345` | Low |
| `AUTO-UI-003` | `TC-UI-PUR-001`, `TC-UI-PUR-007` | Warehouse creates a valid draft purchase | Core warehouse flow with predictable seeded supplier/product data | `Nhà cung cấp 1`, `SP010`, qty `5`, unit cost `1716000` | Medium |
| `AUTO-UI-004` | `TC-UI-INV-001` | Sales creates invoice successfully with in-stock item | Core sales flow with real stock movement and business value | Prefer high-stock sample `SP011`, qty `1`, walk-in customer | Medium |
| `AUTO-UI-005` | `TC-UI-IMP-002` | Product import preview shows valid and invalid row counts | Strong bonus candidate because it exercises file upload, parsing, validation, and row-level feedback | `OSMS-Product-Import-Mixed-Validation.xlsx` | Low |

## UI cases intentionally left manual first

- visual layout and responsive checks
- report page chart interpretation and business summary verification
- destructive flows that are harder to reset quickly, such as repeated invoice cancel / purchase receive / stock rollback sequences
- expired preview cache scenarios such as `TC-UI-IMP-004`, because they are better after core automation is stable
- unsupported image and oversize upload checks unless extra time remains after the five priority flows

## Priority API automation scope

| Auto ID | Related Test Case IDs | Group | Why this should be automated |
|---|---|---|---|
| `AUTO-API-001` | `TC-API-HLT-001` | Health smoke | Fast service-availability gate before any other API or UI run |
| `AUTO-API-002` | `TC-API-CAT-001` to `TC-API-CAT-005` | Catalog happy path queries | Covers pagination, search, category, brand, and sort behaviors |
| `AUTO-API-003` | `TC-API-CAT-006` to `TC-API-CAT-013` | Catalog validation and negative queries | Strong negative coverage for page, pageSize, price range, sort, and out-of-range page |
| `AUTO-API-004` | `TC-API-CAT-014` to `TC-API-CAT-016` | Product detail valid / invalid / not found | Covers `200`, `400`, and `404` behavior in one endpoint family |
| `AUTO-API-005` | `TC-API-CAT-017`, `TC-API-CAT-018` | Trending and filters lookups | Stable lookup endpoints with low maintenance cost |

## Proposed repository structure

```text
Report Test subject/
  automation/
    README.md
    ui/
      README.md
      OSMS.UITests/
        Pages/
        Support/
        TestData/
        Tests/
    api/
      README.md
      postman/
        collections/
        environments/
      newman/
```

## UI Page Object Model plan

### Core pages

- `LoginPage`
  - open login page
  - login with credentials
  - read validation summary
- `AdminDashboardPage`
  - verify successful entry into admin area
- `PurchaseCreatePage`
  - choose supplier
  - add item row
  - set quantity and unit cost
  - submit purchase
- `PurchaseDetailsPage`
  - verify purchase number, status, supplier, and item summary
- `InvoiceCreatePage`
  - choose walk-in or seeded customer
  - choose product
  - set quantity
  - submit invoice
- `InvoiceDetailsPage`
  - verify invoice number and final status
- `ProductImportPage`
  - upload workbook
  - submit preview
- `ProductImportPreviewPage`
  - read total, valid, invalid counts
  - collect row-level error text
- `AccessDeniedPage`
  - verify permission-denied state

### Shared components

- `ToastComponent`
- `SidebarNavComponent`
- `TableRowComponent` for reusable grid handling where selectors repeat

## Reusable utilities plan

- `WebDriverFactory`
  - browser selection by config or environment variable
  - headed vs headless switch
- `AppSettings`
  - base URL
  - browser
  - timeout values
  - screenshot directory
- `WaitHelper`
  - explicit wait wrappers for visibility, clickability, navigation, and toast states
- `AuthHelper`
  - central login helper for admin, sales, warehouse accounts
- `FileUploadHelper`
  - resolve test-data file paths from repo-relative locations
- `ScreenshotHelper`
  - capture on failure and on key checkpoints for evidence reuse
- `TestDataLoader`
  - read UI test data from repo files instead of hard-coding values inside tests

## Browser matrix

| Browser | Target role | Current status | Design decision |
|---|---|---|---|
| `Chrome` | Primary execution and video evidence | Verified installed locally | Implement in Phase 8 |
| `Edge` | Secondary cross-browser run for bonus | `PENDING REAL EXECUTION` because browser binary was not confirmed on this machine | Keep framework browser-agnostic so it can be enabled later without code redesign |
| `Firefox` | Not targeted | Not installed or required for score optimization | Out of scope |

## Manual vs automated split

### Automate first

- repetitive smoke and regression flows
- permission denial checks
- one stable purchase create flow
- one stable invoice create flow
- one stable file-import preview flow
- all public catalog API checks

### Keep manual

- exploratory testing for hidden business bugs
- visual and responsive checks
- long chained business flows that require cleanup after every rerun
- final defect evidence capture where human interpretation is needed

## Design risks and controls

- Some pages do not expose dedicated `data-testid` attributes.
  - Control: hide locator complexity inside page objects and prefer stable `id`, `name`, route, and visible text anchors.
- Purchase and invoice flows mutate database state.
  - Control: use high-stock seeded products and keep automated quantities small.
- Product import preview cache expires after `20` minutes.
  - Control: preview and validation assertions must stay inside one test session.
- Cross-browser bonus is limited by current local browser availability.
  - Control: keep browser selection configurable so Chrome evidence can be delivered first and Edge can be added later if available.

## Phase 8 implementation target

- 1 UI automation project
- 5 priority UI automated tests
- 1 Postman collection
- 1 local Postman environment
- 1 Newman run script
- screenshot and run-instruction support for later evidence capture
