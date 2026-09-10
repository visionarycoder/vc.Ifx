---
name: background-services-channels
title: Background Services with Channels
description: Implement IHostedService and BackgroundService with Channel-based producer/consumer patterns for async work queues.
doc_type: skill
status: active
last_updated: 2026-08-29
target_audience: ai
complexity: medium
estimated_tokens: 1350
prerequisites:
  - ste-agent-writing-standard
  - terminology-dictionary
related_skills:
  - long-running-job-patterns
  - api-infrastructure-bundle
  - message-patterns-dotnet
appliesTo: '**/*.{cs,csproj}'
tags:
  - aspnetcore
  - background-service
  - channels
  - async
  - hosted-service
---
# Background Services with Channels

Agent implements background worker services using `IHostedService`, `BackgroundService`, and `System.Threading.Channels` for async work queues.

## When to Use

| Condition | Agent Action |
|---|---|
| Application processes background work outside request pipeline | Use this skill |
| Application uses producer/consumer pattern for async tasks | Use this skill |
| Application needs graceful shutdown for long-running operations | Use this skill |

## When Not to Use

| Condition | Agent Action |
|---|---|
| Work executes within HTTP request scope only | Use middleware or filters |
| Work requires durable queue (survives restart) | Use Azure Service Bus or RabbitMQ |
| Scheduled work with cron patterns | Use Quartz.NET |

## Core Patterns

| Pattern | Use When | Implementation |
|---|---|---|
| `BackgroundService` | Single long-running loop | Override `ExecuteAsync` with `while (!stoppingToken.IsCancellationRequested)` |
| `IHostedService` | Custom lifecycle control | Implement `StartAsync` and `StopAsync` |
| `Channel<T>` | Producer/consumer queue | Use `Channel.CreateUnbounded<T>()` or bounded |
| Multiple consumers | High throughput needed | Start multiple `BackgroundService` instances reading same channel |

## Workflow

| Step | Agent Action | Test | Pass |
|---|---|---|---|
| 1. Create service | Agent creates class inheriting `BackgroundService`. | Inspect class declaration. | Class inherits `BackgroundService`. |
| 2. Inject channel reader | Agent injects `ChannelReader<T>` into service constructor. | Inspect constructor parameters. | `ChannelReader<T>` parameter exists. |
| 3. Implement loop | Agent overrides `ExecuteAsync` with cancellation-aware loop. | Inspect `ExecuteAsync` method. | Method checks `stoppingToken.IsCancellationRequested`. |
| 4. Register services | Agent registers background service and channel in DI. | Inspect `Program.cs` registrations. | `AddHostedService` and channel singleton exist. |
| 5. Add producer | Agent creates writer methods that write to `ChannelWriter<T>`. | Inspect producer code. | Producer uses `await writer.WriteAsync(item)`. |
| 6. Test shutdown | Agent verifies graceful shutdown behavior. | Stop application during work. | Service completes current item before exit. |

## Implementation Pattern

### Channel Registration

```csharp
var channel = Channel.CreateUnbounded<WorkItem>(new UnboundedChannelOptions
{
    SingleReader = false, // Multiple consumers allowed
    SingleWriter = false  // Multiple producers allowed
});

builder.Services.AddSingleton(channel.Reader);
builder.Services.AddSingleton(channel.Writer);
```

### Background Service

```csharp
public class WorkerService : BackgroundService
{
    private readonly ChannelReader<WorkItem> _reader;
    private readonly ILogger<WorkerService> _logger;
    
    public WorkerService(ChannelReader<WorkItem> reader, ILogger<WorkerService> logger)
    {
        _reader = reader;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _reader.ReadAllAsync(stoppingToken))
        {
            var result = await ProcessItemAsync(item, stoppingToken);
            if (result.Error is not null)
            {
                _logger.LogError(result.Error, "Error processing item {ItemId}", item.Id);
            }
        }
    }
    
    private async Task<WorkItemResult> ProcessItemAsync(WorkItem item, CancellationToken cancellationToken)
    {
        // Process work item
        await Task.Delay(100, cancellationToken); // Placeholder
        return new WorkItemResult(null);
    }
}

public sealed record WorkItemResult(Exception? Error);
```

### Producer Pattern

```csharp
public class WorkQueueService
{
    private readonly ChannelWriter<WorkItem> _writer;
    
    public WorkQueueService(ChannelWriter<WorkItem> writer) => _writer = writer;
    
    public async ValueTask QueueWorkAsync(WorkItem item, CancellationToken cancellationToken = default)
    {
        await _writer.WriteAsync(item, cancellationToken);
    }
}
```

## Configuration Rules

| Rule | Agent Verifies | Fix |
|---|---|---|
| BG-001 | `ExecuteAsync` checks cancellation token | Add `stoppingToken.IsCancellationRequested` checks |
| BG-002 | Exception handling wraps work processing | Add an error boundary around item processing |
| BG-003 | Channel capacity limits prevent unbounded growth when needed | Use `CreateBounded` with capacity limit |
| BG-004 | Graceful shutdown timeout is configured | Set `HostOptions.ShutdownTimeout` |
| BG-005 | Multiple consumers process in parallel when needed | Register multiple `AddHostedService<T>` instances |

## Channel Options

| Option | Use When | Configuration |
|---|---|---|
| Unbounded | Queue size unknown, memory allows growth | `Channel.CreateUnbounded<T>()` |
| Bounded with Wait | Producer blocks when full | `CreateBounded<T>(capacity, BoundedChannelFullMode.Wait)` |
| Bounded with Drop | Drop oldest items when full | `BoundedChannelFullMode.DropOldest` |
| Bounded with Reject | Reject new items when full | `BoundedChannelFullMode.DropWrite` |

## Graceful Shutdown Pattern

```csharp
public override async Task StopAsync(CancellationToken cancellationToken)
{
    _logger.LogInformation("Worker stopping, waiting for current work to complete");
    await base.StopAsync(cancellationToken);
}
```

## Verification Checklist

| Check | Test | Pass |
|---|---|---|
| Service starts | Start application and check logs. | Background service logs startup. |
| Work processes | Queue work item and verify processing. | Item processes successfully. |
| Cancellation works | Stop application during processing. | Service stops gracefully within shutdown timeout. |
| Exception handling | Queue item that throws exception. | Service logs error and continues processing. |
| Channel communication | Producer writes and consumer reads. | Items flow from producer to consumer. |

## Common Pitfalls

| Pitfall | Agent Fix |
|---|---|
| Synchronous blocking in `ExecuteAsync` | Agent uses `await` throughout. |
| No cancellation token checks | Agent adds `stoppingToken.IsCancellationRequested` checks. |
| Unbounded channel with unlimited producers | Agent switches to bounded channel with capacity limit. |
| No exception handling | Agent wraps processing in an error boundary. |
| Missing graceful shutdown | Agent configures `HostOptions.ShutdownTimeout`. |

## Outputs

- `BackgroundService` implementation with cancellation support
- Channel-based producer/consumer registration
- Graceful shutdown configuration
- Exception handling and logging patterns
