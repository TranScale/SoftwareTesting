# Powered by GPT - Software Quality Verification

This folder is the testing submission workspace for the `Online Sales Management System` final project in the Software Testing course.

## System Under Test

- Project: `Online Sales Management System`
- Stack: `ASP.NET Core MVC (.NET 8)`, `EF Core`, `ASP.NET Identity`, `TiDB/MySQL`
- Test surfaces:
  - `Admin UI`
  - `Public catalog UI`
  - `Catalog API`
  - `Health API`

## Team

- Hoang Van Thien - `22D1ITE-SWE03` - `225051915`
- Nguyen Thanh Dat - `22D1ITE-SWE03` - `225050896`
- Le Quang Duy - `22D1ITE-SWE03` - `225051169`

## Purpose Of This Folder

This folder is organized for grading, defense, and traceability. Every deliverable required by the exam brief is mapped to a fixed location so the report, slides, test cases, evidence, automation, and metrics can be reviewed without searching across the source repository.

## Folder Layout

| Folder | Purpose | Final files to place here |
|---|---|---|
| `docs/` | Planning and traceability documents | audit, test plan, test scenarios, mapping, naming rules |
| `report/` | Main report package | final `docx`, exported `pdf` |
| `slides/` | Presentation package | final `pptx` |
| `test-cases/` | UI and API test case workbooks | executed case sheets with member ownership |
| `test-data/` | Reusable test accounts and datasets | UI data, API payloads, account matrix |
| `automation/` | Automation source and run notes | Selenium/Postman scripts, shared utilities |
| `defects/` | Defect log and issue export support | defect log workbook, GitHub Issues export or screenshots |
| `evidence/` | Execution screenshots grouped by purpose | UI, API, defects, report figures |
| `metrics/` | Coverage and summary metrics | module-wise and execution metrics workbooks |
| `results/` | Final execution outputs | pass/fail report, expected vs actual summary |
| `video/` | Automation demonstration | `mp4` or `avi` |

## Current Real Files

- Exam brief: `UEF - Final.pdf`
- Working report draft: `Powered by GPT - Software Quality Verification.docx`
- Canonical report snapshot: `report/Powered by GPT - Software Quality Verification - Final Report.docx`
- Final report PDF: `report/Powered by GPT - Software Quality Verification - Final Report.pdf`
- Report source content: `report/Powered by GPT - Software Quality Verification - Final Report Content.md`
- Presentation source content: `slides/Powered by GPT - Software Quality Verification - Presentation Content.md`
- Final presentation deck: `slides/Powered by GPT - Software Quality Verification - Presentation.pptx`
- UI test cases workbook: `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
- API test cases workbook: `test-cases/api/OSMS-API-Test-Cases.xlsx`
- Final results workbook: `results/OSMS-Final-Test-Results.xlsx`
- Metrics workbook: `metrics/OSMS-Test-Metrics.xlsx`
- Defect log workbook: `defects/exports/OSMS-Defect-Log.xlsx`
- GitHub issue screenshot: `evidence/defects/BUG-20260406-001-github-issue.png`
- Automation video: `video/OSMS-Automation-Demo.mp4`

The working report draft remains at the root of `Report Test subject` for editing stability. The canonical final report path for submission packaging is `report/Powered by GPT - Software Quality Verification - Final Report.docx`.

## Source To Artifact Scope

The submission is based on the real source project in the same repository. The highest-priority modules for testing are:

- authentication and role-based access control
- admin users and permissions
- products, categories, brands, units
- customers and suppliers
- purchases, invoices, stock, reports
- public product catalog
- `GET /api/v1/health`
- `GET /api/v1/catalog/*`

Detailed mapping is maintained in `docs/source-to-testing-mapping.md`.

## Naming And Placement Rules

- Use the exact canonical filenames defined in `docs/file-naming-convention.md`.
- Do not commit Word or PowerPoint temporary lock files.
- Do not store secrets or live credentials in committed test data.
- Do not mark any result as passed without execution evidence.
- Use `PENDING REAL EXECUTION` when a result has not been run yet.

## Recommended Final Appendix Link

Use the GitHub path to this folder as the appendix link source after all required artifacts are populated:

- report
- slides
- test cases
- test data
- results
- evidence
- automation
- video
- defects

## Status

- `Phases 0-15`: source artifacts, metrics, report content, slide content, automation assets, and cleanup checklist are completed
- Current package strengths:
  - structured UI and API test cases
  - reusable test data
  - Selenium and Newman automation assets
  - real UI and API execution evidence
  - real GitHub issue evidence for the confirmed invoice defect
  - final results and metrics workbooks
  - defect workflow and defect log workbook
- Current blockers before a strong final submission:
  - additional execution evidence for `Stock`, `Reports`, `Products`, and `Public Catalog` is still `PENDING REAL EXECUTION`

See `docs/phase-15-final-github-cleanup.md` for the final readiness verdict and submission checklist.
