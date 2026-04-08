# Phase 6 Test Data

## Objective

Prepare reusable, traceable, and evidence-ready test data for UI and API testing without committing real secrets.

## Real data sources used

- `Data/DbSeeder.cs` for demo users, suppliers, customers, categories, units, brands, and generated products
- runtime API observations from the local application for active product IDs, category IDs, brand IDs, stock samples, and catalog totals
- source-code validation rules in `Areas/Admin/Controllers/ProductsController.cs` for product create and Excel import constraints

## Deliverables produced in this phase

- approved seeded demo accounts and negative credential variations
- UI test data dataset in CSV/XLSX format
- API payload and query variation dataset in JSON format
- real local attachment files for upload validation
- mixed-validation product import workbook aligned with the application's actual Excel header contract
- non-`.xlsx` product import sample for extension-rejection testing

## Reusable data highlights

- login accounts: `admin@osms.local`, `sales@osms.local`, `warehouse@osms.local`
- product samples: `SP001`, `SP002`, `SP010`, `SP011`, `SP012`
- catalog filter samples: `categoryId=30012`, `brandId=30039`, `search=SP001`
- seeded supplier and customer names for purchase and invoice flows

## Sensitive data excluded from version control

- `TiDB:Password` from User Secrets
- any external token or deployment credential
- any future personal account or non-coursework credential

## Notes for later execution

- `OSMS-Product-Import-Mixed-Validation.xlsx` is designed to produce both valid and invalid preview rows in the product import screen.
- `OSMS-Product-Import-NonXlsx-Sample.csv` exists only for the extension validation case and should be rejected before parsing.
- Actual preview counts, imported row counts, and screenshots remain `PENDING REAL EXECUTION`.
