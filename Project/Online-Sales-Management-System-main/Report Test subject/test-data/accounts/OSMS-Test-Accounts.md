# OSMS Test Accounts

## Approved Demo Accounts

These accounts are seeded by `Data/DbSeeder.cs` and are intended for coursework testing in the local demo environment.

| Account ID | Role / Group | Email | Password | Status | Reusable | Commit Allowed | Notes |
|---|---|---|---|---|---|---|---|
| `ACC-ADMIN-001` | `Super Admin` | `admin@osms.local` | `Admin@12345` | Active | Yes | Yes | Full access account for admin, invoice, report, and permission flows |
| `ACC-SALES-001` | `Sales Staff` | `sales@osms.local` | `Sales@12345` | Active | Yes | Yes | Use for customer, invoice, report, and permission-denied checks |
| `ACC-WHS-001` | `Warehouse Staff` | `warehouse@osms.local` | `Warehouse@12345` | Active | Yes | Yes | Use for product, supplier, purchase, stock, and permission-denied checks |

## Planned Temporary Accounts

These accounts are intended for later execution support and may need to be created during test runs.

| Account ID | Purpose | Suggested Email | Suggested Password | Commit Allowed | Notes |
|---|---|---|---|---|---|
| `ACC-TEMP-001` | Inactive login validation | `qa.inactive@osms.local` | `QaInactive@12345` | Yes | Create through admin UI, then deactivate before running the inactive-login test |
| `ACC-TEMP-002` | New admin creation test | `qa.hvt.admin01@osms.local` | `QaAdmin@12345` | Yes | Planned data already used in UI test cases |

## Negative Credential Variations

These values are safe to commit because they are fake coursework data, but they should still be tracked as explicit test data rather than reused as real accounts.

| Data ID | Type | Email | Password | Reusable | Notes |
|---|---|---|---|---|---|
| `ACC-NEG-001` | Invalid password | `admin@osms.local` | `WrongAdmin@12345` | Yes | Used for invalid-login verification against a real seeded account |
| `ACC-NEG-002` | Unknown email | `qa.unknown.user@osms.local` | `AnyPassword@12345` | Yes | Used when validating login rejection for a non-existent account |
| `ACC-NEG-003` | Inactive account | `qa.inactive@osms.local` | `QaInactive@12345` | No | Requires real setup during execution before it can be used |

## Sensitive Data Policy

### Safe to commit

- seeded demo login accounts listed above
- fake customer emails such as `customer1@gmail.com`
- fake supplier emails such as `supplier1@partner.com`
- non-production test values created for coursework

### Must not be committed

- `TiDB:Password` from User Secrets
- real deployment credentials
- personal accounts outside seeded demo data
- tokens or secrets generated from external services

## Execution Notes

- The login page visibly exposes the `admin` and `sales` demo accounts in the current UI, so these credentials are already non-secret within the project context.
- Any newly created temporary account should be recorded in final execution evidence if it is used to support a negative case or permission case.
