# TEST SCENARIOS

## 1. Objective

Define the high-level test scenarios for `Online Sales Management System` based on the real application modules, source-code rules, seeded data, and verified local environment. These scenarios are intended to partition coverage for the later UI and API test case phases without unnecessary overlap.

## 2. Scope Basis

The scenario list below is derived from:

- `Program.cs`
- admin controllers under `Areas/Admin/Controllers`
- public controllers under `Controllers`
- API controllers under `Controllers/Api`
- seed data rules in `Data/DbSeeder.cs`
- local verification completed in Phase 0
- test-plan priorities defined in `docs/test-plan.md`

## 3. Scenario Design Rules

- Every critical module includes at least one positive and one negative or edge scenario.
- Permission-sensitive modules are represented through explicit authorization scenarios.
- Business-rule-heavy modules such as `Purchases`, `Invoices`, and `Stock` prioritize lifecycle and consistency checks.
- API scenarios cover both successful and validation/error behaviors.
- Scenarios are intentionally high level; later phases will split them into detailed executable test cases.

## 4. Scenario Matrix

| Scenario ID | Module | Interface | Scenario | Flow Class | Priority | Test Type |
|---|---|---|---|---|---|---|
| `SCN-AUTH-001` | Authentication | Admin UI | Login succeeds with a valid seeded admin account and reaches the authorized admin area | Positive | Critical | UI Functional |
| `SCN-AUTH-002` | Authentication | Admin UI | Login rejects invalid credentials and keeps the user on the login form with error feedback | Negative | Critical | UI Validation |
| `SCN-AUTH-003` | Authentication | Admin UI | Login blocks an inactive admin account even when credentials are otherwise valid | Negative | High | UI Validation / Security |
| `SCN-GOV-001` | Permissions | Admin UI | `sales` role is denied access to `Purchases` and `Admins` modules | Permission | Critical | UI Authorization |
| `SCN-GOV-002` | Permissions | Admin UI | `warehouse` role is denied access to `Invoices` and `Reports` modules | Permission | Critical | UI Authorization |
| `SCN-GOV-003` | Permissions | Admin UI | Full admin account can access core management modules needed for system operation | Positive | High | UI Authorization |
| `SCN-GOV-004` | Admin Users | Admin UI | Admin user creation succeeds with valid email, password, active status, and group assignment | Positive | High | UI Functional |
| `SCN-GOV-005` | Admin Users | Admin UI | Current user cannot deactivate or delete their own admin account | Negative | High | UI Authorization / Edge |
| `SCN-GOV-006` | Admin Groups | Admin UI | Permission matrix updates affect role access while Super Admin protections remain enforced | Edge | High | UI Permission / Business Rule |
| `SCN-CUS-001` | Customers | Admin UI | Customer creation succeeds with valid data and the details view shows the created customer information | Positive | High | UI Functional |
| `SCN-CUS-002` | Customers | Admin UI | Customer form rejects invalid or incomplete input such as missing name or malformed email | Negative | High | UI Validation |
| `SCN-PROD-001` | Products | Admin UI | Product creation succeeds with valid category, unit, brand, and numeric values | Positive | Critical | UI Functional |
| `SCN-PROD-002` | Products | Admin UI | Product create or edit blocks duplicate SKU values for active products | Negative | Critical | UI Validation |
| `SCN-PROD-003` | Products | Admin UI | Product form blocks invalid category, unit, or brand references and negative numeric inputs | Negative | Critical | UI Validation |
| `SCN-PROD-004` | Products | Admin UI | Product image upload blocks unsupported extensions or oversized files | Negative | High | UI Validation |
| `SCN-PROD-005` | Products | Admin UI | Product trending toggle changes the trending state for an active product | Edge | High | UI Business Rule |
| `SCN-PROD-006` | Product Import | Admin UI | Product Excel preview rejects non-xlsx, oversized, empty, or malformed uploads | Negative | Critical | UI Validation |
| `SCN-PROD-007` | Product Import | Admin UI | Product Excel confirm imports valid rows and handles missing or expired preview cache safely | Edge | Critical | UI Business Rule |
| `SCN-SUP-001` | Suppliers | Admin UI | Supplier creation succeeds with valid data and the supplier remains available for purchase-related flows only when active | Positive | Medium | UI Functional |
| `SCN-PUR-001` | Purchases | Admin UI | Draft purchase creation succeeds with an active supplier and at least one valid item | Positive | Critical | UI Functional |
| `SCN-PUR-002` | Purchases | Admin UI | Purchase form rejects missing supplier, missing date, or empty item list | Negative | Critical | UI Validation |
| `SCN-PUR-003` | Purchases | Admin UI | Receiving a draft purchase increases stock and records stock movement history | Edge | Critical | UI Business Rule |
| `SCN-PUR-004` | Purchases | Admin UI | Cancel logic prevents invalid state transitions, especially for already received purchases | Negative | Critical | UI Business Rule |
| `SCN-INV-001` | Invoices | Admin UI | Invoice creation succeeds with valid items for a selected customer or walk-in sale | Positive | Critical | UI Functional |
| `SCN-INV-002` | Invoices | Admin UI | Invoice form rejects empty item lists or invalid customer selection | Negative | Critical | UI Validation |
| `SCN-INV-003` | Invoices | Admin UI | Invoice creation uses server-side sale price rather than trusting client-posted price values | Edge | Critical | UI Business Rule / Security |
| `SCN-INV-004` | Invoices | Admin UI | Invoice creation fails cleanly when stock is insufficient and avoids stock corruption | Negative | Critical | UI Business Rule |
| `SCN-INV-005` | Invoices | Admin UI | Recording payment updates paid amount and status transitions correctly | Edge | Critical | UI Business Rule |
| `SCN-INV-006` | Invoices | Admin UI | Invoice cancellation returns stock, records movement, and safely handles repeated or non-standard cancellation attempts | Edge | Critical | UI Business Rule |
| `SCN-STK-001` | Stock | Admin UI | Low-stock screen lists products at or below reorder threshold | Positive | High | UI Functional |
| `SCN-STK-002` | Stock | Admin UI | Stock movement history supports product/date filtering and export generation | Positive | High | UI Functional |
| `SCN-REP-001` | Reports | Admin UI | Report screen handles normal and reversed date ranges while excluding cancelled transactions from totals | Edge | High | UI Business Rule |
| `SCN-REP-002` | Reports | Admin UI | Report export workbook matches on-screen summary totals and detailed invoice or purchase sections | Positive | High | UI Functional |
| `SCN-PUB-001` | Public Catalog | Public UI | Public product list supports search plus category and brand filtering | Positive | High | UI Functional |
| `SCN-PUB-002` | Public Catalog | Public UI | Public product list supports price filtering and sort options without invalid navigation or empty-state confusion | Edge | High | UI Functional |
| `SCN-PUB-003` | Public Catalog | Public UI | Public product details page shows the selected product information and navigates correctly from listing views | Positive | Medium | UI Functional |
| `SCN-API-001` | Health API | API | Health endpoint returns service status and server metadata for smoke verification | Positive | Medium | API Smoke |
| `SCN-API-002` | Catalog API | API | Product list endpoint returns paginated active products for valid query combinations | Positive | High | API Functional |
| `SCN-API-003` | Catalog API | API | Product list endpoint returns validation errors for invalid page, page size, price range, or sort parameters | Negative | High | API Validation |
| `SCN-API-004` | Catalog API | API | Product detail endpoint returns the expected payload for a valid active product ID | Positive | High | API Functional |
| `SCN-API-005` | Catalog API | API | Product detail endpoint handles invalid ID values and not-found products correctly | Negative | High | API Validation |
| `SCN-API-006` | Catalog API | API | Trending and filters endpoints return active category, brand, and product data with the expected structure | Positive | High | API Functional |

## 5. Coverage Summary

### 5.1 Total Coverage

- Total scenarios: `40`
- Admin UI scenarios: `31`
- Public UI scenarios: `3`
- API scenarios: `6`

### 5.2 Coverage By Risk Type

- Positive scenarios: authentication, CRUD success paths, report export, public browsing, and API happy paths
- Negative scenarios: invalid credentials, invalid forms, duplicate SKU, invalid API queries, insufficient stock
- Permission scenarios: `sales`, `warehouse`, and admin-role boundary checks
- Edge or business-rule scenarios: payment transition logic, stock updates, cancellation rules, reversed report dates, cached Excel preview behavior

### 5.3 Critical Coverage Modules

The strongest scenario concentration is intentionally placed on:

- authentication
- permissions
- products
- purchases
- invoices
- stock
- reports
- catalog API

These modules carry the highest business and grading value for later detailed test case design.

## 6. Scenario Notes For Later Phases

- `SCN-INV-006` is important for real bug hunting because invoice cancellation logic is business-critical and potentially sensitive to non-standard statuses.
- `SCN-PUB-002` is important because the public catalog sorting UI has source-code indicators of possible rendering issues and should be executed carefully.
- `SCN-GOV-006` should be linked closely with role-based test data and evidence because it strengthens the rubric score for authorization coverage and bug management readiness.
- `SCN-PROD-006` and `SCN-PROD-007` are high-value because Excel import validation and preview cache behavior are harder to test casually and often reveal real defects.

## 7. Readiness For Phase 4 And Phase 5

This scenario set is intentionally broader than the minimum required case count so the next phases can:

- split UI scenarios into non-overlapping member-owned test cases
- separate API scenarios into dedicated endpoint-focused cases
- prioritize critical paths first
- keep enough negative and edge coverage for strong rubric scoring

`PENDING REAL EXECUTION`: scenario coverage is designed and source-backed, but execution status and evidence will only be finalized in later phases.
