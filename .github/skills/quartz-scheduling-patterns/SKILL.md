---
name: quartz-scheduling-patterns
description: Configure Quartz.NET triggers with explicit cron, interval, timezone, and misfire patterns.
title: Quartz.NET Scheduling Patterns
doc_type: skill
status: active
last_updated: 2026-07-29
target_audience: ai
complexity: high
estimated_tokens: 1450
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - quartz-job-authoring
  - quartz-calendar-schedules
appliesTo: '**/*.{cs,csproj,json,md}'
tags:
  - quartz
  - scheduling
  - patterns
---
# Quartz.NET Scheduling Patterns

Agent configures Quartz triggers with explicit timing, timezone, and recovery behavior.

## When to Use

| Condition | Use |
|---|---|
| Agent defines recurring trigger timing | Use this skill |
| Agent selects cron, simple, calendar interval, or daily interval triggers | Use this skill |
| Agent reviews timezone or misfire behavior | Use this skill |
| Agent validates next fire times before deployment | Use this skill |

## When Not to Use

| Condition | Use |
|---|---|
| Work depends on holiday or business-day rules | Use `quartz-calendar-schedules` |
| Work starts immediately with no recurring trigger | Trigger the job directly |
| Work depends on external events | Use queue or event-driven patterns |
| Work needs sub-second precision | Use a different scheduler |

## Required Inputs

| Input | Required | Notes |
|---|---|---|
| Trigger intent | Yes | Fixed time, fixed interval, business window, or calendar interval |
| Expression or interval | Yes | Cron string or interval settings |
| Timezone | Yes | Explicit production timezone |
| Misfire policy | Yes | Skip, catch up once, or catch up aggressively |
| Validation window | No | Range of future fire times to inspect |

## Required Workflow

Agent performs these steps:
1. Agent selects the trigger type from the pattern table before writing code.
2. Agent sets an explicit timezone for production triggers.
3. Agent sets a misfire policy that matches business impact.
4. Agent previews future fire times before deployment.
5. Agent documents the intended firing pattern beside the trigger definition.

Test: Agent runs `dotnet build [project].csproj` and evaluates representative future fire times with Quartz trigger preview code.
Pass: Agent observes zero compile errors. Agent confirms that previewed fire times match the intended recurrence.

## Trigger Pattern Matrix

| Need | Preferred pattern | Notes |
|---|---|---|
| Fixed clock time | `WithCronSchedule(...)` | Use for daily, weekly, monthly, or quarterly timing |
| Fixed elapsed interval | `WithSimpleSchedule(...)` | Use for every N seconds, minutes, or hours |
| Calendar-aware interval | `WithCalendarIntervalSchedule(...)` | Use for week or month boundaries and DST-aware intervals |
| Repeating business window | `WithDailyTimeIntervalSchedule(...)` | Use for repeated firing inside daily hours |

## Cron Pattern Table

| Intent | Expression | Meaning |
|---|---|---|
| Daily at 02:00 | `0 0 2 * * ?` | One fire every day at 02:00 |
| Weekdays at 09:00 | `0 0 9 ? * MON-FRI` | One fire on business weekdays |
| Every 15 minutes, weekdays, 09:00-17:59 | `0 0/15 9-17 ? * MON-FRI` | Repeating business-hours fire pattern |
| First Monday at 08:00 | `0 0 8 ? * MON#1` | One fire on the first Monday |
| Month end at 23:59 | `0 59 23 L * ?` | One fire on the last day of the month |
| Quarter start at 00:00 | `0 0 0 1 1,4,7,10 ?` | One fire on each quarter boundary |

## Timezone and Misfire Matrix

| Concern | Preferred pattern | Avoid |
|---|---|---|
| Timezone | `InTimeZone(TimeZoneInfo.FindSystemTimeZoneById(...))` | Implicit server local time |
| DST-sensitive daily windows | `WithDailyTimeIntervalSchedule(...)` or `WithCalendarIntervalSchedule(...)` | Cron that hides DST intent |
| Missed fire with safe skip | `WithMisfireHandlingInstructionDoNothing()` | Catch-up flood for non-critical work |
| Missed fire with one catch-up run | `WithMisfireHandlingInstructionFireAndProceed()` | Silent loss when one catch-up run matters |
| Flood-tolerant replay | `WithMisfireHandlingInstructionIgnoreMisfires()` | Use on expensive or stateful work |

## Verification Matrix

| Test | Run | Pass |
|---|---|---|
| Compile verification | `dotnet build [project].csproj` | Zero compile errors |
| Cron validation | `CronExpression.IsValidExpression("...")` in existing test or preview code | Expression returns `true` |
| Fire-time preview | `GetFireTimeAfter(...)` loop for representative dates | Preview matches documented intent |
| Timezone verification | Existing scheduler test or trigger preview under configured timezone | Preview reflects the configured timezone |

## Verification Checklist

Agent verifies:
- [ ] Trigger type matches the recurrence need
- [ ] Trigger identity is explicit and unique
- [ ] Trigger references a job with `.ForJob(jobKey)`
- [ ] Production trigger sets an explicit timezone
- [ ] Misfire policy matches business impact
- [ ] Previewed fire times match the documented intent

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Cron uses both day-of-month and day-of-week | Set one field to `?` |
| Trigger relies on server local time | Set `InTimeZone(...)` explicitly |
| Simple interval crosses DST unexpectedly | Use calendar interval or daily interval pattern |
| Misfire replay floods downstream systems | Use `DoNothing` or `FireAndProceed` |
| Trigger intent stays undocumented | Add a plain-language recurrence note beside the trigger |
