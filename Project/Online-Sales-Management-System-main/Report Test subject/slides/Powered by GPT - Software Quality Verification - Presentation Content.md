# Powered by GPT - Software Quality Verification

## Presentation Goal

Use this file as the source content for the final `PPTX`. The deck is intentionally short and defense-oriented. It highlights the strongest evidence currently available and avoids overclaiming unexecuted coverage.

## Recommended Deck Size

- main deck: `10` slides
- optional backup: `1` Q&A slide
- target speaking time: `8-10 minutes`

## Slide 1. Title

### Slide content

**Powered by GPT - Software Quality Verification**

- System Under Test: `Online Sales Management System`
- Course: `Software Testing`
- Team:
  - Hoang Van Thien
  - Nguyen Thanh Dat
  - Le Quang Duy
- Repository:
  - `github.com/Alucard30Dec/Online-Sales-Management-System`

### Visual to show

- simple cover layout with team names and repository name

## Slide 2. Project And Test Scope

### Slide content

**What We Tested**

- Admin UI
- Public Product Catalog
- Health API
- Catalog API

**Highest-Risk Business Areas**

- Authentication and permissions
- Products and product import
- Purchases
- Invoices
- Stock and reports

### Visual to show

- one scope diagram or four-box layout for `Admin UI`, `Public UI`, `Health API`, `Catalog API`

## Slide 3. Test Strategy

### Slide content

**Strategy**

- source-based project audit, not generic template testing
- black-box execution + white-box-informed test design
- manual testing for business logic and visible validation
- targeted automation for high-value regression paths

**Automation Stack**

- UI: `.NET 8 + xUnit + Selenium`
- API: `Postman + Newman`
- structure: `Page Object Model + reusable helpers`

### Visual to show

- one small architecture row:
  - `Source Audit -> Test Design -> Manual + Automation -> Evidence -> Metrics`

## Slide 4. Coverage And Team Allocation

### Slide content

**Design Coverage**

- `42` documented scenarios
- `63` total test cases
  - `44` UI
  - `19` API
- `100%` scenario design coverage

**Team Allocation**

- Hoang Van Thien: `17` UI cases
- Nguyen Thanh Dat: `13` UI cases
- Le Quang Duy: `14` UI cases

### Visual to show

- bar chart or simple table for case ownership
- small note:
  - `42 / 42` scenarios are now mapped to test cases

## Slide 5. Automation Implementation

### Slide content

**Implemented Automation Assets**

- Selenium UI project with `Page Objects`
- shared config, wait helpers, screenshot capture
- Postman collection with Newman runner

**Priority Automated Flows**

- admin login smoke
- sales permission check
- purchase creation
- invoice creation
- product import preview
- API health and catalog groups

### Visual to show

- folder tree screenshot or simplified structure:
  - `Pages`
  - `Support`
  - `Tests`
  - `postman`
  - `newman`

## Slide 6. Real Execution Evidence

### Slide content

**Confirmed Pass Evidence**

- `TC-UI-AUTH-001` passed
- `TC-UI-AUTH-003` passed
- `TC-UI-IMP-002` passed
- `TC-UI-PUR-001` and `TC-UI-PUR-007` passed
- full API collection passed (`19 / 19`)

**Confirmed Fail Evidence**

- `TC-UI-INV-001` failed with UI and server-log proof

**Important Rule**

- no unexecuted test was marked as pass
- only one issue was promoted into a confirmed defect because the root cause was proven by server log evidence

### Visual to show

- login success screenshot:
  - `evidence/ui/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- purchase success screenshot:
  - `evidence/ui/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- invoice defect screenshot:
  - `evidence/ui/automation/20260406_053902_TC-UI-INV-001-failure.png`
- Newman output snippet for full collection:
  - `results/automation-api/newman-full-run.txt`

## Slide 7. Current Metrics

### Slide content

**Execution Summary As Of 2026-04-06**

- total cases: `63`
- executed: `25`
- pass: `24`
- fail: `1`
- blocked: `0`
- not run: `38`
- execution progress: `39.68%`

**Interface View**

- Admin UI: `6` executed, `5` pass, `1` fail
- API: `19` executed, `19` pass

### Visual to show

- `evidence/report/OSMS-Test-Metrics-Summary.png`

## Slide 8. Defect And Risk Analysis

### Slide content

**Confirmed Defects**

- `1` confirmed defect
  - `BUG-20260406-001` in invoice creation
  - tracked in GitHub Issue `#1`

**Closed Observations**

- authorization redirect behavior
- product import preview locator issue

**Highest Business Risks Still Unverified**

- Stock
- Products
- Reports
- Public Catalog UI

### Visual to show

- one risk heat-style table:
  - `Verified`
  - `Confirmed Defect`
  - `Unverified`
- `evidence/defects/BUG-20260406-001-github-issue.png`

## Slide 9. Key Insights

### Slide content

**What We Can Defend**

- project is testable and automation-capable
- source-based test design is strong
- traceability from scenario -> test case -> result -> evidence -> defect is in place

**What We Do Not Overclaim**

- current evidence still does not prove full system stability
- one core business defect is currently confirmed
- more UI execution is still required for high-confidence quality claims

### Visual to show

- two-column slide:
  - `Proven Today`
  - `Still Pending`

## Slide 10. Next Steps And Submission Package

### Slide content

**Highest-Value Next Steps**

1. fix and retest invoice creation through GitHub Issue `#1`
2. execute stock, reports, products, and public-catalog UI flows
3. add optional cross-browser evidence if Edge is available
4. refresh the report, slides, and metrics only if new execution evidence is added

**Submission Package**

- Final report
- PPTX
- test cases
- test data
- automation scripts
- final results
- metrics
- evidence
- video

### Visual to show

- GitHub appendix link:
  - `github.com/Alucard30Dec/Online-Sales-Management-System/tree/main/Report%20Test%20subject`

## Slide 11. Q&A Backup

### Slide content

**Likely Questions**

- Why do you have `0` confirmed defects?
- Why is execution progress still low?
- Why did you choose Selenium and Newman?
- How do you distinguish automation failure from product defect?
- Which module is riskiest right now?

### Visual to show

- plain clean backup slide only
