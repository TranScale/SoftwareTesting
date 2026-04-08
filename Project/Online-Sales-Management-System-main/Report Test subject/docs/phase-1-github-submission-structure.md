# PHASE 1 - GITHUB SUBMISSION STRUCTURE

## Objective

Create a grading-friendly testing submission structure inside `Report Test subject` of the current repository. The structure must separate planning, cases, data, evidence, automation, defects, metrics, final results, report, slides, and video so that the examiner can trace each claim in the report back to a real artifact.

## Canonical Folder Tree

```text
Report Test subject/
  README.md
  UEF - Final.pdf
  Powered by GPT - Software Quality Verification.docx
  docs/
    phase-0-project-audit.md
    phase-1-github-submission-structure.md
    file-naming-convention.md
    source-to-testing-mapping.md
  report/
  slides/
  test-cases/
    ui/
    api/
  test-data/
    ui/
    api/
    accounts/
  automation/
    ui/
    api/
    shared/
  defects/
    github-issues/
    exports/
  evidence/
    ui/
    api/
    defects/
    reports/
  metrics/
  results/
  video/
```

## Exact Artifact Placement

| Folder | Exact target files |
|---|---|
| `docs/` | `phase-0-project-audit.md`, `test-plan.md`, `test-scenarios.md`, `defect-workflow.md`, traceability docs |
| `report/` | `Powered by GPT - Software Quality Verification - Final Report.docx`, `Powered by GPT - Software Quality Verification - Final Report.pdf` |
| `slides/` | `Powered by GPT - Software Quality Verification - Presentation.pptx` |
| `test-cases/ui/` | `OSMS-UI-Test-Cases.xlsx` |
| `test-cases/api/` | `OSMS-API-Test-Cases.xlsx` |
| `test-data/ui/` | `OSMS-UI-Test-Data.xlsx` |
| `test-data/api/` | `OSMS-API-Test-Data.json` |
| `test-data/accounts/` | `OSMS-Test-Accounts.md` |
| `automation/ui/` | Selenium project, page objects, run notes |
| `automation/api/` | Postman collections, environments, Newman scripts |
| `automation/shared/` | reusable helpers, config, fixtures |
| `defects/github-issues/` | issue screenshots showing severity, priority, and status |
| `defects/exports/` | `OSMS-Defect-Log.xlsx` or exported issue evidence |
| `evidence/ui/` | UI execution screenshots mapped to test case IDs |
| `evidence/api/` | API response screenshots and runner outputs |
| `evidence/defects/` | defect reproduction screenshots and comparison images |
| `evidence/reports/` | charts or screenshots inserted into report/slides |
| `metrics/` | `OSMS-Test-Metrics.xlsx` |
| `results/` | `OSMS-Final-Test-Results.xlsx` |
| `video/` | `OSMS-Automation-Demo.mp4` or `OSMS-Automation-Demo.avi` |

## Structure Decisions

### 1. Keep the testing package inside the current repository

This avoids split ownership between the source project and the testing artifacts. It also makes appendix linking easier because the report can reference one repository instead of two.

### 2. Separate `test-cases`, `test-data`, `results`, and `evidence`

This is required for grading clarity:

- `test-cases` answers what was designed
- `test-data` answers what inputs were prepared
- `results` answers what happened during execution
- `evidence` proves the execution and defect findings

### 3. Split UI and API assets

The project has both UI and API scope, but they are not equally broad. Splitting them prevents mixed workbooks and makes it easier to defend coverage decisions during presentation.

### 4. Keep defects separate from evidence

`defects/` stores the defect management artifacts. `evidence/defects/` stores reproduction screenshots. This separation is important because the report must reference both the issue tracker and the visual proof.

## Current Placement Notes

- The live exam brief stays at the root of `Report Test subject`.
- The active report draft currently stays at the root of `Report Test subject`.
- When the report content stabilizes, the final copy should be placed in `report/`.

## Review Outcome

This structure is ready for the next phases:

- Phase 2 can write the test plan into `docs/`
- Phases 3-6 can fill `test-cases/` and `test-data/`
- Phases 8-9 can populate `automation/`, `results/`, `evidence/`, and `video/`
- Phase 15 can finalize packaging without having to redesign the directory layout
