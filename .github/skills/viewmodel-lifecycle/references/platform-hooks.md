---
title: View Model Lifecycle Platform Hooks
doc_type: reference
status: active
last_updated: 2026-08-30
target_audience: ai
estimated_tokens: 211
related_docs:
  - ../SKILL.md
---
# View Model Lifecycle Platform Hooks

| Platform | Common Hooks | Cleanup Focus |
|---|---|---|
| WPF | Window or page load and unload events, Prism navigation, region activation | Event handlers, dispatcher timers, messenger cleanup |
| .NET MAUI | `OnAppearing`, `OnDisappearing`, Shell navigation callbacks | Sensor subscriptions, platform services, reentry behavior |
| WinUI 3 | `OnNavigatedTo`, `OnNavigatedFrom`, window activation, app lifecycle events | Event tokens, dispatcher queues, dialogs, cancellation tokens |
| UWP | Activation, suspension, resume, and frame navigation events | Suspension state, event tokens, background-task coordination |
