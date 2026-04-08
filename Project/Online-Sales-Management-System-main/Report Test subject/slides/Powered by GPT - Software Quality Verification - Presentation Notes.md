# Presentation Notes

## Objective

Use these notes while building the slide deck and during oral defense. The notes are optimized for short, direct delivery and for handling likely examiner questions.

## Delivery Strategy

- keep the deck under `10` main slides
- spend most time on:
  - strategy
  - real evidence
  - metrics
  - insights
- do not spend too long reading tables
- do not claim broad stability
- clearly separate:
  - what was designed
  - what was executed
  - what is still pending

## Slide-By-Slide Speaking Points

### Slide 1. Title

- Introduce the project, team, and repository.
- State that the submission is based on the real source code of `Online Sales Management System`.

### Slide 2. Project And Test Scope

- Explain that the project has both UI and API surfaces.
- Emphasize that the scope was chosen based on real business-critical modules, not random screens.
- Mention that permissions, purchases, invoices, stock, and product import were treated as high-risk areas.

### Slide 3. Test Strategy

- Explain the mixed approach:
  - manual for business rules and visual validation
  - API checks for stable endpoint behavior
  - automation for high-value repeatable flows
- Mention that source-code analysis was used to design stronger edge cases.

### Slide 4. Coverage And Team Allocation

- State that the team produced `42` scenarios and `63` total test cases.
- Mention that each member had at least `10` UI test cases.
- If asked about non-overlap, explain module ownership split:
  - HVT focused more on auth, permissions, admin, invoices, reports
  - NTD focused on customers, suppliers, purchases
  - LQD focused on products, import, stock

### Slide 5. Automation Implementation

- Explain why Selenium + Newman were selected:
  - practical with current stack
  - bonus-friendly
  - maintainable
- Mention `Page Object Model` and reusable helpers as the main quality point.

### Slide 6. Real Execution Evidence

- This is one of the strongest slides.
- Show that UI and API both have real pass evidence.
- Explicitly say that only the invoice failure was promoted into a confirmed defect because the server log proved the root cause.
- This makes the submission more credible.

### Slide 7. Current Metrics

- Explain that `63` cases exist and `25` now have real execution evidence.
- Say clearly that the package is materially stronger than a smoke-only baseline, but it is still not full regression stability.
- This answer is stronger than pretending the whole project is already fully verified.

### Slide 8. Defect And Risk Analysis

- Explain that `1` confirmed defect now exists because the invoice failure has both UI proof and server-log proof.
- Mention that the biggest unresolved execution gaps are now stock, reports, products, and public-catalog UI.
- Also mention that the earlier authorization and import observations were closed by focused reruns, so they should not be presented as product bugs.

### Slide 9. Key Insights

- Summarize the main honest conclusion:
  - strong planning
  - strong traceability
  - real automation evidence exists
  - but execution depth is still limited
- This is the slide that shows analytical maturity.

### Slide 10. Next Steps And Submission Package

- Explain the immediate next actions:
  - fix and retest invoice creation through GitHub Issue `#1`
  - execute the remaining highest-value UI modules
  - optionally add cross-browser evidence
- End by pointing to the GitHub appendix link containing the deliverables.

### Slide 11. Q&A Backup

- Keep this hidden unless asked.

## Demo Order

If the examiner allows a short live demo or asks for concrete proof, use this order:

1. show the GitHub folder structure in `Report Test subject`
2. show `test-cases/ui/OSMS-UI-Test-Cases.xlsx`
3. show `automation/ui/OSMS.UITests`
4. show login success screenshot
5. show Newman full-collection output
6. show `results/OSMS-Final-Test-Results.xlsx`
7. show `metrics/OSMS-Test-Metrics.xlsx`

This order starts from structure, then evidence, then summary, which is easier to defend than starting from theory.

## Evidence To Show On Screen

### Strongest evidence already available

- `evidence/ui/automation/20260406_053930_TC-UI-AUTH-001-success.png`
- `evidence/ui/automation/20260406_054245_TC-UI-AUTH-003-access-denied.png`
- `evidence/ui/automation/20260406_054115_TC-UI-IMP-002-preview.png`
- `evidence/ui/automation/20260406_054004_TC-UI-PUR-001-draft-created.png`
- `results/automation-api/newman-full-run.txt`
- `results/automation-ui/auth-permission-rerun.trx`
- `results/automation-ui/import-preview-rerun.trx`
- `results/automation-ui/purchase-rerun.trx`
- `results/OSMS-Final-Test-Results.xlsx`
- `metrics/OSMS-Test-Metrics.xlsx`

### Strongest defect proof

- `evidence/ui/automation/20260406_053902_TC-UI-INV-001-failure.png`
- `evidence/defects/BUG-20260406-001-invoice-create-log.txt`

These two files should be presented together because the screenshot shows the user-facing failure and the log proves the root cause.

## Likely Q&A And Recommended Answers

### Why do you have 1 confirmed defect?

Because invoice creation now has both a reproducible UI failure and a matching server-side exception trace. That is strong enough to justify a real defect log.

### Why is execution progress 39.68% instead of 100%?

Because the package still avoids fake completion. We expanded execution where evidence could be collected credibly, especially the full API surface and several high-value UI flows, but we did not mark the remaining UI areas as executed without proof.

### Why did you choose Selenium and Newman?

The project stack is `.NET 8`, so Selenium with `.NET + xUnit` is practical and maintainable. The exposed API surface is small and stable, so Postman/Newman gives fast repeatable coverage with low setup cost.

### Which module is riskiest right now?

The highest current business risk is invoice creation because it has one confirmed defect, followed by still-unexecuted areas such as stock, reports, and public-catalog UI.

### How do you distinguish automation failure from product defect?

A product defect must have a reproducible expected-versus-actual mismatch. If the runner times out or the script expectation is unstable, we keep it as an observation. Only the invoice failure crossed that threshold because the server log confirmed the backend root cause.

### What is the strongest part of your submission?

The strongest part is the traceable structure: source-based audit, real test cases, real automation assets, real evidence files, and metrics that do not overstate the result.

### Why were the earlier authorization and import failures not logged as bugs?

Because focused reruns showed that the authorization case was a valid redirect-based denial and the import case was an automation locator issue. The team only promoted the invoice case into a confirmed defect after the UI failure and server log matched.

## Slide Design Guidance

- use short titles, not paragraphs
- keep each slide to `3-5` bullets
- prioritize screenshots and numbers over text walls
- use one color for `Pass`, one for `Blocked`, one for `Pending`
- if time is short, Slides `3`, `6`, `7`, `9`, and `10` are the highest-value core
