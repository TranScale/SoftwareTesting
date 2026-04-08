# Phase 10 Defect Log And Bug Management

## Objective

Define a professional defect-management process for the current OSMS submission and prepare a traceable defect register that stays fully aligned with real evidence collected so far.

## Current defect state as of 2026-04-06

- Confirmed product defects: `1`
- Open observations from automation execution: `0`
- Closed observations: `2`
- Rejected or duplicate defects: `0`

The repository now contains one confirmed product defect with both UI and server-log evidence. The two older automation observations were reclassified as closed observations after focused reruns proved that they were not product bugs.

## Real record classification used in this project

- `Confirmed Defect`
  - reproducible application behavior
  - expected result is clear
  - actual result is proven by screenshots, logs, or API evidence
- `Observation`
  - unusual execution result exists
  - product bug is not confirmed yet
  - needs manual retest or environment validation
- `Rejected`
  - not a bug, expected behavior, duplicate, or test-script issue

## Required defect fields

Every confirmed issue in GitHub Issues and every exported defect row must contain:

- defect ID or GitHub issue number
- title
- defect type
- module
- related test case ID
- interface (`UI` or `API`)
- environment
- browser or runner
- build or execution date
- severity
- priority
- status
- assigned owner
- precondition
- steps to reproduce
- expected result
- actual result
- attachment list
- root-cause hint or notes

## Environment format to use

Use this exact environment structure in every defect report:

- Application: `Online Sales Management System`
- Repo: `Alucard30Dec/Online-Sales-Management-System`
- Environment: `Local test environment`
- URL: `http://localhost:5068`
- OS: `Windows 11`
- Browser: `Chrome`
- Database tag: `TiDB / test`
- Execution date: actual run date such as `2026-04-05`
- Evidence source: `Manual`, `Selenium xUnit`, or `Postman Newman`

## Severity definition

- `severity:critical`
  - system unavailable, data corruption, or a core business flow cannot continue
- `severity:high`
  - major business flow fails, permission leak exists, or financial or stock data becomes unreliable
- `severity:medium`
  - important function behaves incorrectly but a workaround may exist
- `severity:low`
  - minor incorrect behavior, cosmetic problem, or non-blocking validation issue

## Priority definition

- `priority:p1`
  - fix immediately before demo or submission rerun
- `priority:p2`
  - fix in the current test cycle
- `priority:p3`
  - fix after critical flows are stable
- `priority:p4`
  - optional or cosmetic backlog item

## Severity and priority guidance for this project

- Auth bypass or incorrect permission access:
  - expected label baseline: `severity:high`, `priority:p1`
- Purchase or invoice creation creates wrong totals, wrong stock movement, or cannot submit:
  - expected label baseline: `severity:critical` or `severity:high`, `priority:p1`
- Product import accepts invalid rows silently:
  - expected label baseline: `severity:high`, `priority:p1` or `priority:p2`
- Catalog API validation returns wrong status or malformed body:
  - expected label baseline: `severity:medium`, `priority:p2`
- UI alignment or non-blocking label issue:
  - expected label baseline: `severity:low`, `priority:p4`

## GitHub Issues workflow

1. Execute or re-execute the related test case.
2. Decide whether the outcome is `Confirmed Defect`, `Observation`, or `Automation Script Failure`.
3. If the issue is not yet reproducible manually, keep it in the defect register as `Observation`.
4. If the issue is reproducible manually or through stable API evidence, open a GitHub Issue.
5. Apply label groups for severity, priority, status, module, and interface.
6. Attach screenshots, runner output, and testcase linkage.
7. Update `results/execution-evidence-mapping.csv` with the GitHub issue link.
8. When fixed, re-run the original testcase and add retest evidence before closing the issue.

## GitHub status workflow

- `status:new`
- `status:triaged`
- `status:in-progress`
- `status:ready-for-retest`
- `status:closed`
- `status:rejected`

Recommended movement:

- new evidence -> `status:new`
- validated by QA lead -> `status:triaged`
- assigned to developer -> `status:in-progress`
- developer reports fix -> `status:ready-for-retest`
- QA retest passes -> `status:closed`
- not reproducible / expected behavior / script problem -> `status:rejected`

## Attachments required for each confirmed defect

- one issue screenshot from GitHub Issues showing labels and title
- one screenshot showing the page or request context before the failure if needed
- one screenshot showing the incorrect result
- related runner output file if automation detected the problem
- related API response snippet if the issue is API-related

## Exact screenshots that must appear in the final report if defects exist

- GitHub Issue page screenshot with:
  - issue number
  - title
  - severity label
  - priority label
  - status label
  - module label
- failure-state screenshot from the application or API client
- retest screenshot after fix if the issue is closed before submission

## Current record summary

### OBS-20260405-001

- related testcase: `TC-UI-AUTH-003`
- module: `Authorization / Purchases`
- classification: `Observation`
- current status: `Closed - Behavior Confirmed`
- reason:
  - focused rerun confirmed the real application behavior is a redirect away from `/Admin/Purchases`
  - this satisfies the intended denial rule for the testcase
  - the original failure came from an automation expectation mismatch, not a product bug

### OBS-20260405-002

- related testcase: `TC-UI-IMP-002`
- module: `Products Import`
- classification: `Observation`
- current status: `Closed - Automation Fixed`
- reason:
  - focused rerun passed after narrowing the automation submit locator to the correct import form
  - the application preview flow is now confirmed working for this case

### BUG-20260406-001

- related testcase: `TC-UI-INV-001`
- module: `Invoices`
- classification: `Confirmed Defect`
- current status: `Open - Confirmed`
- GitHub Issue: `#1`
- GitHub URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/issues/1`
- reason:
  - UI rerun returned to the Create page with the toast `Failed to create invoice. Please check data and try again.`
  - server log excerpt proves `InvalidOperationException` caused by using a user-initiated transaction with `MySqlRetryingExecutionStrategy`
  - the issue is reproducible with valid walk-in invoice data in the current environment
- issue evidence:
  - `evidence/defects/BUG-20260406-001-github-issue.png`

## Immediate bug-management actions

1. Keep `defects/exports/OSMS-Defect-Register.csv` and `results/execution-evidence-mapping.csv` aligned with GitHub Issue `#1`.
2. Retest invoice creation after any fix and capture the post-fix details-page evidence.
3. Update the issue status labels when development work begins or a retest becomes available.
