---
name: quartz-calendar-schedules
title: Quartz.NET Calendar-Based Schedules
description: Build Quartz.NET Schedule definitions that respect holidays, payroll calendars, business-day shifts, and organization-specific date rules.
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1400
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - quartz-job-authoring
  - quartz-scheduling-patterns
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - quartz
  - calendar
  - schedules
  - business-days
---
# Quartz.NET Calendar-Based Schedules

Agent defines Quartz Schedule behavior that depends on holidays, business days, payroll dates, or fiscal rules.

## When to Use

| Condition | Use |
|---|---|
| Agent excludes holidays or shutdown dates from a Schedule | Use this skill |
| Agent shifts a Schedule date forward or backward on non-business days | Use this skill |
| Agent models payroll, month-end, quarter-end, or fiscal-date recurrence | Use this skill |
| Agent combines Quartz triggers with reusable calendar rules | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Recurrence depends only on fixed clock timing | Use `quartz-scheduling-patterns` |
| Work runs once with no recurrence | Trigger the job directly |
| Work starts from external events | Use queue or event-driven patterns |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Schedule rule set | Yes | Holidays, business days, payroll dates, fiscal periods, or blackout dates |
| Base trigger | Yes | Cron, daily interval, or calendar interval trigger |
| Shift policy | Yes | Skip, move earlier, or move later |
| Business-day definition | Yes | Weekend set, local holidays, and exceptions |
| Validation dates | No | Boundary dates near weekends, month end, and year end |

## Required Workflow

Agent performs these steps:
1. Agent models the Schedule rule set before writing trigger code.
2. Agent selects the Quartz calendar type from the rule table.
3. Agent keeps reusable Schedule rules in shared calendar registration.
4. Agent defines one explicit shift policy for excluded dates.
5. Agent validates representative boundary dates before deployment.

Test: Agent runs `dotnet build [project].csproj` and previews representative trigger fire times against the configured calendar rules.
Pass: Agent observes zero compile errors. Agent confirms that previewed fire times match the documented Schedule rules and shift policy.

## Calendar Rule Table

| Need | Preferred pattern | Notes |
|---|---|---|
| One-off holiday exclusions | `HolidayCalendar` | Use for ad hoc blackout dates |
| Annual recurring exclusions | `AnnualCalendar` | Use for fixed annual holidays |
| Weekday-only recurrence | `WeeklyCalendar` | Use for weekend exclusion |
| Combined business-day logic | Chained weekday plus holiday calendars | Keep rules reusable |
| Domain-specific included dates | Custom `BaseCalendar` implementation | Use for payroll or fiscal-date logic |

## Recurrence Pattern Table

| Recurrence intent | Preferred approach | Reason |
|---|---|---|
| Daily or weekly clock timing | Cron trigger plus calendar exclusion | Trigger keeps timing. Calendar keeps date logic. |
| Payroll date with business-day shift | Custom calendar or date calculator plus trigger | Payroll recurrence depends on domain rules |
| Month-end with holiday skip | Cron trigger for month-end plus holiday calendar | Trigger expresses month boundary. Calendar expresses exclusion. |
| Quarter-end with move-earlier rule | Trigger plus shift calculator | Shift policy stays explicit and testable |

## Schedule Rule Matrix

| Concern | Preferred rule | Avoid |
|---|---|---|
| Terminology | Agent uses `Schedule` for domain concepts | Mixed terms such as schedule and scheduling in the same rule set |
| Rule reuse | Agent registers calendars once and references them from triggers | Duplicated holiday logic in each trigger |
| Shift behavior | Agent defines skip, move earlier, or move later in one place | Implicit date movement |
| Boundary testing | Agent tests weekend adjacency, month end, quarter end, and year end | Happy-path-only validation |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Calendar rule verification | Existing scheduler test or preview code with representative dates | Preview matches documented Schedule rules |
| Shift-policy verification | Existing test or targeted date-calculator test | Excluded dates move exactly as documented |
| Naming verification | Review changed markdown and code comments | `Schedule` terminology stays consistent |

## Verification Checklist

Agent verifies:
- [ ] Schedule rules identify holidays, weekends, and exception dates explicitly
- [ ] Quartz calendar type matches the rule set
- [ ] Shared calendars replace duplicated date logic
- [ ] Shift policy is explicit and testable
- [ ] Boundary-date previews match the intended Schedule behavior
- [ ] `Schedule` terminology stays consistent

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Cron encodes business-day logic by itself | Move date logic into Quartz calendars or a dedicated calculator |
| Each trigger repeats the same holiday list | Register one shared calendar |
| Shift behavior stays implicit | Document and test one shift policy |
| Payroll recurrence looks like ordinary weekday timing | Keep payroll logic in domain-specific Schedule rules |
