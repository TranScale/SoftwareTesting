# API Automation

Framework: `Postman Collection + Newman`

## Implemented artifacts

- collection: `postman/collections/OSMS-API-Automation.postman_collection.json`
- environment: `postman/environments/OSMS-Local.postman_environment.json`
- runner: `newman/run-api-tests.ps1`

## Coverage

- `TC-API-HLT-001`
- `TC-API-CAT-001` to `TC-API-CAT-018`

## Run

```powershell
pwsh "Report Test subject/automation/api/newman/run-api-tests.ps1"
```

To override the base URL:

```powershell
pwsh "Report Test subject/automation/api/newman/run-api-tests.ps1" -BaseUrl "http://localhost:5068"
```

## Preconditions

- the OSMS application is running locally
- `npx` is available
- internet access is available the first time `newman` is resolved through `npx`
