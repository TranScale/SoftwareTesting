# Automation Workspace

This folder contains the implemented automation artifacts for the OSMS coursework submission.

## Implemented stacks

- UI automation: `.NET 8 + xUnit + Selenium WebDriver`
- API automation: `Postman Collection + Newman`

## Current scope

### UI automated flows

- `TC-UI-AUTH-001`: admin login smoke
- `TC-UI-AUTH-003`: sales access denied to Purchases
- `TC-UI-PUR-001`, `TC-UI-PUR-007`: create draft purchase and verify details
- `TC-UI-INV-001`: create walk-in invoice successfully
- `TC-UI-IMP-002`: preview product import workbook and verify valid/invalid counts

### API automated groups

- health smoke
- catalog happy path queries
- catalog validation and negative queries
- product detail valid / invalid / not found
- trending and filters lookup

## Quick commands

### UI automation

```powershell
pwsh "Report Test subject/automation/ui/run-ui-tests.ps1"
```

### API automation

```powershell
pwsh "Report Test subject/automation/api/newman/run-api-tests.ps1"
```
