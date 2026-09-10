---
description: Microsoft Orleans actor model and distributed systems best practices
applyTo: '**/orleans/**,**/*grain*,**/*silo*,**/*orleans*'
---

# Orleans Instructions

## Scope
Applies to Microsoft Orleans 9.2.1 actor model development, grain implementation, and distributed system architecture.

## Project-Specific Patterns
- **Grain hierarchy**: Individual component grains → Group grains → System orchestrator grain
- **Grain interfaces**: All inherit from `IGrainWithGuidKey` for consistent identity management
- **Group grains**: Use `Guid.Empty` for singleton group grain instances
- **Grain communication**: Use `GrainFactory.GetGrain<T>(id)` for grain-to-grain calls
- **Testing**: Use `Orleans.TestingHost 9.2.1` with TestCluster for grain integration tests

## Grain Development
- Design grains with clear state boundaries and responsibilities.
- Use `IGrainWithGuidKey` for grain interfaces (project standard).
- Implement proper state persistence and recovery mechanisms.
- Handle grain activation and deactivation lifecycle properly.
- Use immutable state objects to prevent concurrency issues.
- Follow pattern: Interface in `Grain.Abstractions/`, implementation in `Grains/`

## State Management
- Use Orleans persistence providers for durable state storage.
- Implement proper state validation and consistency checks.
- Handle state migration for schema evolution.
- Use appropriate consistency models (strong vs eventual).
- Implement proper error handling for state operations.

## Grain Communication Patterns
- Use grain references for inter-grain communication.
- Implement proper timeout and retry strategies.
- Handle grain unavailability and network partitions.
- Use streaming for high-throughput scenarios.
- Implement proper backpressure and flow control.

## Silo Configuration
- Configure appropriate silo membership and clustering.
- Use proper service discovery and load balancing.
- Configure appropriate resource limits and scaling policies.
- Implement proper health checks and monitoring.
- Use appropriate networking and security configurations.

## Performance Optimization
- Use grain timers appropriately for scheduled operations.
- Implement proper batching for high-frequency operations.
- Use grain observers for efficient event distribution.
- Optimize grain placement and locality strategies.
- Monitor and tune garbage collection and memory usage.

## Fault Tolerance and Resilience
- Implement proper error handling and recovery strategies.
- Use circuit breaker patterns for external dependencies.
- Handle silo failures and grain migration gracefully.
- Implement proper distributed consensus when needed.
- Use compensation patterns for distributed transactions.

## Model Extensions Pattern
- Create static extension classes in `Model.Extensions/` for computed properties and formatting
- Implement health assessment logic as extension methods (e.g., `IsHealthy()`)
- Use hardware-specific thresholds: CPU 85°C, GPU 83°C, Motherboard 70°C, Storage 60°C + 90% usage
- Extension methods enable testability without modifying core models
- Follow pattern: `public static bool IsHealthy(this TModel model) => condition;`

## Testing Strategies
- Use `Orleans.TestingHost 9.2.1` for grain testing with TestCluster.
- Use MSTest 4.0.1 with FluentAssertions 8.8.0 for assertions.
- Test grain interface contracts separately from implementations.
- Integration test grain interactions and workflows.
- Test failure scenarios and recovery mechanisms.
- Validate threshold boundaries precisely (e.g., 84.99°C vs 85.0°C vs 85.01°C).
- Test implementation drift prevention (static class contracts, method signatures).
- Load test under realistic concurrent access patterns.
- Test clustering and failover scenarios.

## Streaming and Events
- Use Orleans streams for decoupled event processing.
- Implement proper stream lifecycle management.
- Handle stream failures and reprocessing scenarios.
- Use appropriate stream providers for different scenarios.
- Implement proper event ordering and deduplication.

## Security Best Practices
- Implement proper authentication and authorization.
- Use secure communication protocols between silos.
- Validate grain method parameters and state changes.
- Implement proper audit logging for sensitive operations.
- Use principle of least privilege for grain permissions.

## Monitoring and Observability
- Implement structured logging with correlation IDs.
- Use Orleans telemetry and metrics for monitoring.
- Monitor grain activation patterns and performance.
- Implement distributed tracing across grain calls.
- Set up alerting for cluster health and performance issues.

## Deployment and Operations
- Use appropriate container orchestration for silo deployment.
- Implement proper rolling updates and blue-green deployments.
- Configure appropriate persistence and clustering providers.
- Use infrastructure as code for reproducible deployments.
- Implement proper backup and disaster recovery strategies.

## Advanced Patterns
- Use virtual grains for stateless operations.
- Implement grain services for singleton behaviors.
- Use interceptors for cross-cutting concerns.
- Implement proper versioning strategies for grain interfaces.
- Use code generation for efficient serialization.