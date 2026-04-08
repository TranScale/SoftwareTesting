# Automation Video Capture Checklist

## Target output

- `video/OSMS-Automation-Demo.mp4`

## Completion status

- Recording re-created on `2026-04-06`
- Verified duration: `130 seconds`
- Verified file size: approximately `9.58 MB`

## Exact recording order

1. Show the repository root and open `Report Test subject/automation`.
2. Show the UI automation command:
   - `powershell -ExecutionPolicy Bypass -File "Report Test subject/automation/ui/run-ui-tests.ps1" -Filter FullyQualifiedName~AdminLoginSmokeSucceeds`
3. Show the browser launching and finishing one stable UI flow:
   - `TC-UI-AUTH-001`
4. Show where the screenshot was saved:
   - `evidence/ui/automation/20260406_053930_TC-UI-AUTH-001-success.png`
5. Show the API automation command:
   - `powershell -ExecutionPolicy Bypass -File "Report Test subject/automation/api/newman/run-api-tests.ps1"`
6. Show the Newman summary finishing successfully.
7. Show the saved API result files:
   - `results/automation-api/newman-full-run.txt`
   - `results/automation-api/newman-results.xml`
8. Show the final results workbook:
   - `results/OSMS-Final-Test-Results.xlsx`
9. Keep the full video between `2` and `4` minutes.

## Recording rules

- Use the current local environment only.
- Do not splice in fake screens or non-project footage.
- If a run fails during recording, restart and keep only the clean successful capture.
- If audio is unnecessary, silent video is acceptable.
