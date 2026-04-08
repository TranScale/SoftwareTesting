# GitHub Issue Label Taxonomy

Use these labels consistently for OSMS defect tracking.

## Severity labels

- `severity:critical`
- `severity:high`
- `severity:medium`
- `severity:low`

## Priority labels

- `priority:p1`
- `priority:p2`
- `priority:p3`
- `priority:p4`

## Status labels

- `status:new`
- `status:triaged`
- `status:in-progress`
- `status:ready-for-retest`
- `status:closed`
- `status:rejected`

## Module labels

- `module:auth`
- `module:permissions`
- `module:dashboard`
- `module:products`
- `module:product-import`
- `module:stock`
- `module:purchases`
- `module:invoices`
- `module:reports`
- `module:catalog-api`
- `module:health-api`

## Interface labels

- `interface:ui`
- `interface:api`
- `interface:automation`

## Triage helper labels

- `type:defect`
- `type:observation`
- `type:automation-script`
- `type:duplicate`
- `type:enhancement`

## Recommended label sets

### Example for a confirmed permission bug

- `type:defect`
- `interface:ui`
- `module:permissions`
- `severity:high`
- `priority:p1`
- `status:new`

### Example for a confirmed API validation bug

- `type:defect`
- `interface:api`
- `module:catalog-api`
- `severity:medium`
- `priority:p2`
- `status:new`

### Example for a runner problem that is not a product defect

- `type:automation-script`
- `interface:automation`
- `severity:low`
- `priority:p3`
- `status:rejected`
