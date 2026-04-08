# UI Automation

Framework: `.NET 8 + xUnit + Selenium WebDriver`

Primary browser: `Chrome` via Selenium Manager

## Implemented tests

- `AuthenticationTests`
  - `TC-UI-AUTH-001`
  - `TC-UI-AUTH-003`
- `PurchaseFlowTests`
  - `TC-UI-PUR-001`
  - `TC-UI-PUR-007`
- `InvoiceFlowTests`
  - `TC-UI-INV-001`
- `ProductImportTests`
  - `TC-UI-IMP-002`

## Project structure

```text
ui/
  run-ui-tests.ps1
  OSMS.UITests/
    appsettings.json
    Pages/
    Support/
    Tests/
```

## Run

### Default run

```powershell
pwsh "Report Test subject/automation/ui/run-ui-tests.ps1"
```

### Headless run

```powershell
pwsh "Report Test subject/automation/ui/run-ui-tests.ps1" -Headless
```

### Filter one test case

```powershell
pwsh "Report Test subject/automation/ui/run-ui-tests.ps1" -Filter "CaseId=TC-UI-AUTH-001"
```

## Preconditions

- the OSMS application is running locally at `http://localhost:5068`
- seeded demo accounts are available
- Chrome is installed
- phase 6 test data files remain in their current repo paths
