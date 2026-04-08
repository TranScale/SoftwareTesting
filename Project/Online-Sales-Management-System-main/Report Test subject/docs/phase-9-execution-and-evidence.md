# Phase 9 Execution And Evidence

## Objective

Prepare a real, traceable execution-and-evidence package for the current OSMS submission without fabricating pass results, bug reports, screenshots, or automation claims.

## Evidence basis used in this phase

### Real execution completed on 2026-04-06

- Focused UI reruns executed from `Report Test subject/automation/ui/run-ui-tests.ps1`
  - result files:
    - `results/automation-ui/auth-permission-rerun.trx`
    - `results/automation-ui/import-preview-rerun.trx`
    - `results/automation-ui/purchase-rerun.trx`
    - `results/automation-ui/invoice-rerun.trx`
  - covered outcomes:
    - `TC-UI-AUTH-001` passed
    - `TC-UI-AUTH-003` passed
    - `TC-UI-IMP-002` passed
    - `TC-UI-PUR-001` passed
    - `TC-UI-PUR-007` passed
    - `TC-UI-INV-001` failed and became a confirmed defect
- Full API automation run executed with Newman
  - scope: complete Postman collection
  - result: all `19` API test cases passed
  - result files:
    - `results/automation-api/newman-full-run.txt`
    - `results/automation-api/newman-results.xml`

### Real screenshots currently available

- `evidence/ui/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- `evidence/ui/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
- `evidence/ui/automation/20260406_054115_TC-UI-IMP-002-preview.png`
- `evidence/ui/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- `evidence/ui/automation/20260406_053902_TC-UI-INV-001-failure.png`

### Evidence interpretation rules applied

- `Pass` is used only when a real execution result exists and the expected behavior is visibly confirmed.
- `Fail` must be reserved for a confirmed product defect with reproducible expected-versus-actual mismatch.
- `Automation Script Failure` is used when the runner failed or timed out, but the product defect is not yet confirmed.
- `PENDING REAL EXECUTION` is used for any test or artifact that has not been executed or captured yet.

## Current confirmed execution status

### Confirmed pass evidence

- `TC-UI-AUTH-001`
  - real screenshot shows successful login to the admin dashboard as `admin@osms.local`
  - focused UI rerun passed
- `TC-UI-AUTH-003`
  - direct navigation to `/Admin/Purchases` by `sales@osms.local` was rejected by redirect away from the protected route
  - server log also showed `GET /Admin/Purchases` returned `302`
- `TC-UI-IMP-002`
  - mixed-validation workbook preview opened successfully and displayed `6` total rows, `1` valid row, and `5` invalid rows
- `TC-UI-PUR-001` and `TC-UI-PUR-007`
  - a valid draft purchase was created successfully and the details page assertions passed
- all API cases `TC-API-HLT-001` to `TC-API-CAT-018`
  - full Newman collection run passed with saved text and JUnit artifacts

### Confirmed product defect evidence

- `TC-UI-INV-001`
  - valid walk-in invoice creation failed in the current environment
  - UI stayed on the Create page and showed `Failed to create invoice. Please check data and try again.`
  - focused rerun TRX and extracted server log prove `BUG-20260406-001`

## Execution checklist

- Confirm application base URL is reachable before every run.
- Record execution date, tester, browser, OS, environment, and test scope.
- Use the seeded accounts from `test-data/accounts/OSMS-Test-Accounts.md`.
- Keep screenshots in timestamped subfolders or timestamped filenames.
- Export runner outputs after every execution batch.
- Update `results/execution-evidence-mapping.csv` immediately after each run.
- If a test fails, decide first whether it is:
  - a confirmed product defect
  - a blocked environment issue
  - an automation script failure
- Only create a defect entry after reproducing the issue with a stable expected-versus-actual mismatch.

## Screenshot checklist

### Mandatory screenshots for the final submission

- successful admin login landing page
- successful API smoke runner summary
- each confirmed product defect:
  - one screenshot before action if context matters
  - one screenshot showing the triggering action or input
  - one screenshot showing the actual incorrect result
- one screenshot of any permission-denied behavior if that behavior is confirmed manually
- purchase creation success detail page
- invoice creation success detail page
- product import preview page showing valid and invalid row counts
- one screenshot showing the final results workbook or metrics summary
- one screenshot showing the issue tracker entry for each confirmed defect

### Screenshots that already exist

- login success screenshot for `TC-UI-AUTH-001`
- permission-denial screenshot for `TC-UI-AUTH-003`
- import preview screenshot for `TC-UI-IMP-002`
- purchase details screenshot for `TC-UI-PUR-001` and `TC-UI-PUR-007`
- invoice failure screenshot for `TC-UI-INV-001`
- GitHub Issue screenshot for `BUG-20260406-001`
- metrics summary screenshot for the final report and slide deck

### Screenshots still missing

- `PENDING REAL EXECUTION`: additional business-flow evidence for `Stock`, `Reports`, `Products`, and `Public Catalog`

## Bug evidence checklist

- Reproduce the issue with stable steps and environment details.
- Capture exact test case ID and module.
- Capture expected result and actual result in separate sentences.
- Record severity and priority with business impact rationale.
- Attach screenshots that show context and failure outcome.
- If the issue is API-related, attach request, response status, and response body snippet.
- If the issue is UI-related, attach browser, URL, account role, and visible page state.
- If the failure is from automation only, mark it as `Automation Script Failure` and do not open a product defect yet.

## Video evidence checklist

- Show the repository path and the automation folder used.
- Show the command used to run UI automation.
- Show the browser executing at least one stable automated UI flow.
- Show the final runner result summary.
- Show where screenshots and result files are stored after the run.
- Show the Newman API run or collection runner summary.
- Export final video as `mp4` or `avi`.

## Result capture mapping rule

The file `results/execution-evidence-mapping.csv` is the source-of-truth traceability sheet for:

- `Test Case ID`
- current execution result
- evidence screenshot path
- result artifact path
- video status
- bug or defect linkage

Use these status values consistently:

- `Pass`
- `Fail`
- `Blocked`
- `Not Run`
- `Automation Script Failure`

## Immediate next evidence priority

1. Fix and retest invoice creation, then capture the post-fix details-page evidence if the defect is resolved.
2. Execute and capture the remaining high-value UI modules: `Stock`, `Reports`, `Products`, and `Public Catalog`.
3. Keep the metrics summary screenshot, GitHub issue, and automation video linked consistently across the report, slides, and appendix.

## Known limitations after this phase

- UI execution depth is still partial even though the API surface is now fully executed.
- Full business confidence still depends on retesting invoice creation and running additional UI modules.
- Cross-browser evidence is still not available.
