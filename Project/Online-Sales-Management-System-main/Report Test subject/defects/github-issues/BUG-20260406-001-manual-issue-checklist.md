# BUG-20260406-001 Manual GitHub Issue Checklist

## Completion status

- Issue created on `2026-04-06`
- GitHub issue number: `#1`
- GitHub URL: `https://github.com/Alucard30Dec/Online-Sales-Management-System/issues/1`
- Labels applied:
  - `severity:high`
  - `priority:high`
  - `status:open`
  - `module:invoices`
  - `interface:web-ui`
  - `type:defect`
- Screenshot saved:
  - `evidence/defects/BUG-20260406-001-github-issue.png`

## Issue title

- `Invoice creation fails for valid walk-in invoice with in-stock item`

## Required labels

- `severity:high`
- `priority:high`
- `status:open`
- `module:invoices`
- `interface:web-ui`
- `type:defect`

## Required body sections

1. Environment
   - `Local test environment`
   - `http://localhost:5068`
   - `Google Chrome`
2. Preconditions
   - admin user logged in
   - `/Admin/Invoices/Create` reachable
   - active product `SP010` with stock available
3. Steps to reproduce
   - log in as `admin@osms.local`
   - open `/Admin/Invoices/Create`
   - keep customer as walk-in
   - add product `SP010` with quantity `1`
   - submit the invoice
4. Expected result
   - invoice is created successfully
   - stock is reduced
   - user is redirected to invoice details
5. Actual result
   - create action stays on the Create page
   - toast shows `Failed to create invoice. Please check data and try again.`
   - server log shows `InvalidOperationException` in `InvoicesController.Create`

## Required attachments

- `evidence/ui/automation/20260406_053902_TC-UI-INV-001-failure.png`
- `results/automation-ui/invoice-rerun.trx`
- `evidence/defects/BUG-20260406-001-invoice-create-log.txt`

## After opening the issue

1. Copy the issue URL into:
   - `defects/exports/OSMS-Defect-Register.csv`
   - `results/execution-evidence-mapping.csv`
2. Capture one screenshot of the issue page with labels visible.
3. Save the screenshot under:
   - `evidence/defects/BUG-20260406-001-github-issue.png`
