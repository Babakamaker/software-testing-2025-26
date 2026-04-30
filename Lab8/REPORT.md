# Lab 8 CI/CD Report

## Branch naming convention

The workflow accepts branch names that match:

```text
^(feature|bugfix|hotfix|release|chore|docs|test|ci)/[a-z0-9][a-z0-9._-]*$
```

This convention keeps the branch purpose visible in the prefix and keeps the rest of the name safe for GitHub Actions, shell scripts, and issue references. Examples: `feature/orders-api`, `bugfix/order-validation`, `ci/k6-workflow`.

## Branch protection

Branch protection for `main`:

- Require a pull request before merging.
- Require status checks to pass before merging.
- Required status check: `build-and-test` from `pr-tests.yml`.

Screenshot: add the GitHub Settings -> Branches screenshot here after enabling the rule.

## Workflow runs

- Branch Name Check: add successful Actions URL after push.
- CI: add successful Actions URL after push to `main`.
- PR Tests: add successful Actions URL from the pull request.
- Migration: add successful Actions URL from the pull request.
- k6 Performance Tests: add successful Actions URL from the pull request or manual run.

## k6 SLO

Smoke test SLO:

- `http_req_failed < 1%`
- `p95(http_req_duration) < 500ms`
- `checks > 99%`

The smoke test runs on every pull request, so it must be fast and strict enough to catch obvious regressions without slowing down review. The selected SLO focuses on the read path of `GET /api/orders`, where a healthy API should return quickly with near-zero failures in a single-user smoke scenario.

Manual performance profiles:

- Load: `p95 < 750ms`, failures `< 2%`
- Stress: `p95 < 1000ms`, failures `< 5%`
- Spike: `p95 < 1500ms`, failures `< 10%`

These profiles intentionally relax latency and error thresholds as concurrency rises, while still making regressions visible in `results.json`.

## Merged PR

Add the merged pull request link here after all required checks pass.
