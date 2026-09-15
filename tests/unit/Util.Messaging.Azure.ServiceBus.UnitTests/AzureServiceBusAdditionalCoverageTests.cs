using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Ifx.Messaging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Util.Messaging;

namespace Util.Messaging.Azure.ServiceBus.UnitTests;

[TestClass]
public sealed class AzureServiceBusAdditionalCoverageTests
{
    #region Message Bus Tests

    [TestMethod]
    public async Task CreateConsumer_WhenDefaultHandlerConsumesMessage_ShouldCompleteWithoutCustomHandler()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);
        IMessageConsumer consumer = bus.CreateConsumer<TestMessage>("billing");

        await consumer.StartAsync();
        await client.QueueProcessor.RaiseMessageAsync(TestData.CreateContext(new TestMessage { OrderNumber = "SO-3001" }));

        client.QueueProcessor.StartCalls.Should().Be(1);
        client.QueueProcessor.Contexts.Should().ContainSingle().Which.CompleteCalls.Should().Be(1);
        await ((IAsyncDisposable)consumer).DisposeAsync();
    }

    [TestMethod]
    public void Constructor_WhenServiceBusClientIsNull_ShouldThrowArgumentNullException()
    {
        Action action = () => _ = new AzureServiceBusMessageBus(
            (ServiceBusClient)null!,
            Options.Create(TestData.CreateOptions()));

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("client");
    }

    [TestMethod]
    public void Publisher_WhenAccessedFromPublicConstructor_ShouldReturnCurrentInstance()
    {
        AzureServiceBusMessageBus bus = new(
            new RecordingSdkServiceBusClient(),
            Options.Create(TestData.CreateOptions()));

        bus.Publisher.Should().BeSameAs(bus);
    }

    [TestMethod]
    public void CreateConsumer_WhenHandlerIsNull_ShouldThrowArgumentNullException()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);

        Action action = () => _ = bus.CreateConsumer<TestMessage>("billing", null!);

        action.Should().Throw<ArgumentNullException>()
            .Which.ParamName.Should().Be("handler");
    }

    [TestMethod]
    public async Task PublishAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        AzureServiceBusMessageBus bus = CreateBus(new FakeServiceBusClient());

        Func<Task> action = () => bus.PublishAsync<TestMessage>(null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public async Task PublishAsync_WhenCorrelationIdIsInvalid_ShouldPublishWithoutCorrelationId()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);

        await bus.PublishAsync(new TestMessage { CorrelationId = "not-a-guid" });

        ServiceBusMessage message = client.Sender.SentMessages.Should().ContainSingle().Subject;
        message.CorrelationId.Should().Be("not-a-guid");

        using JsonDocument document = JsonDocument.Parse(message.Body.ToString());
        document.RootElement.GetProperty("correlationId").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [TestMethod]
    public async Task PublishAsync_WhenSenderDisposesAsynchronously_ShouldAwaitDisposal()
    {
        AsyncDisposingServiceBusClient client = new();
        AzureServiceBusMessageBus bus = new(
            client,
            Options.Create(TestData.CreateOptions()));

        await bus.PublishAsync(new TestMessage());

        client.Sender.DisposeCalls.Should().Be(1);
        client.Sender.DisposeCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task PublishAsync_WhenSendCompletesAsynchronously_ShouldAwaitOperation()
    {
        AsyncCompletingServiceBusClient client = new();
        AzureServiceBusMessageBus bus = new(
            client,
            Options.Create(TestData.CreateOptions()));

        await bus.PublishAsync(new TestMessage());

        client.Sender.SendCalls.Should().Be(1);
        client.Sender.SendCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task PublishAsync_WhenClientReturnsNullSender_ShouldThrowNullReferenceException()
    {
        AzureServiceBusMessageBus bus = new(
            new NullSenderServiceBusClient(),
            Options.Create(TestData.CreateOptions()));

        Func<Task> action = () => bus.PublishAsync(new TestMessage());

        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreNull_ShouldThrowArgumentNullException()
    {
        AzureServiceBusMessageBus bus = CreateBus(new FakeServiceBusClient());

        Func<Task> action = () => bus.PublishBatchAsync<TestMessage>(null!);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("messages");
    }

    [TestMethod]
    public async Task PublishBatchAsync_WhenMessagesAreEmpty_ShouldNotCreateSender()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageBus bus = CreateBus(client);

        await bus.PublishBatchAsync(Array.Empty<TestMessage>());

        client.LastSenderEntityPath.Should().BeNull();
        client.Sender.SendAttempts.Should().Be(0);
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenMessageIsNull_ShouldThrowArgumentNullException()
    {
        AzureServiceBusMessageBus bus = CreateBus(new FakeServiceBusClient());

        Func<Task> action = () => bus.ScheduleAsync<TestMessage>(null!, DateTimeOffset.UtcNow);

        var assertions = await action.Should().ThrowAsync<ArgumentNullException>();
        assertions.Which.ParamName.Should().Be("message");
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenSenderDisposesAsynchronously_ShouldAwaitDisposal()
    {
        AsyncDisposingServiceBusClient client = new();
        AzureServiceBusMessageBus bus = new(
            client,
            Options.Create(TestData.CreateOptions()));

        await bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.AddMinutes(1));

        client.Sender.DisposeCalls.Should().Be(1);
        client.Sender.DisposeCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenScheduleCompletesAsynchronously_ShouldAwaitOperation()
    {
        AsyncCompletingServiceBusClient client = new();
        AzureServiceBusMessageBus bus = new(
            client,
            Options.Create(TestData.CreateOptions()));

        await bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.AddMinutes(1));

        client.Sender.ScheduleCalls.Should().Be(1);
        client.Sender.ScheduleCompleted.Should().BeTrue();
    }

    [TestMethod]
    public async Task ScheduleAsync_WhenClientReturnsNullSender_ShouldThrowNullReferenceException()
    {
        AzureServiceBusMessageBus bus = new(
            new NullSenderServiceBusClient(),
            Options.Create(TestData.CreateOptions()));

        Func<Task> action = () => bus.ScheduleAsync(new TestMessage(), DateTimeOffset.UtcNow.AddMinutes(1));

        await action.Should().ThrowAsync<NullReferenceException>();
    }

    [TestMethod]
    public void DeserializeMessage_WhenEnvelopeIsNull_ShouldThrowJsonException()
    {
        Action action = () => _ = AzureServiceBusMessageBus.DeserializeMessage<TestMessage>(BinaryData.FromString("null"));

        action.Should().Throw<JsonException>()
            .WithMessage("The Service Bus message body did not contain an envelope.");
    }

    [TestMethod]
    public void DeserializeMessage_WhenPayloadIsNull_ShouldThrowJsonException()
    {
        BinaryData body = BinaryData.FromString(
            """
            {
              "messageId": "f5738ec7-e9c9-41a5-9364-c6993c2588db",
              "enqueuedAtUtc": "2026-09-15T12:00:00Z",
              "messageType": "test",
              "message": null
            }
            """);

        Action action = () => _ = AzureServiceBusMessageBus.DeserializeMessage<TestMessage>(body);

        action.Should().Throw<JsonException>()
            .WithMessage("The Service Bus message body did not contain a * payload.");
    }

    #endregion

    #region Options Tests

    [TestMethod]
    public void TopicOptions_WhenValidated_ShouldExposeTopicEntityPathAndProcessorOptions()
    {
        AzureServiceBusOptions options = new()
        {
            FullyQualifiedNamespace = "contoso.servicebus.windows.net",
            TopicName = "orders",
            MaxConcurrentCalls = 3,
            PrefetchCount = 7,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(9),
            InitialRetryDelay = TimeSpan.Zero
        };

        string entityPath = options.GetEntityPath();
        ServiceBusProcessorOptions processorOptions = options.CreateProcessorOptions();

        entityPath.Should().Be("orders");
        options.UsesQueue().Should().BeFalse();
        processorOptions.AutoCompleteMessages.Should().BeFalse();
        processorOptions.MaxConcurrentCalls.Should().Be(3);
        processorOptions.PrefetchCount.Should().Be(7);
        processorOptions.MaxAutoLockRenewalDuration.Should().Be(TimeSpan.FromMinutes(9));
    }

    [TestMethod]
    public void Validate_WhenInitialRetryDelayIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        AzureServiceBusOptions options = new()
        {
            ConnectionString = TestData.ConnectionString,
            QueueName = "orders",
            InitialRetryDelay = TimeSpan.FromMilliseconds(-1)
        };

        Action action = () => options.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(AzureServiceBusOptions.InitialRetryDelay));
    }

    [TestMethod]
    public void Validate_WhenMaxAutoLockRenewalDurationIsNegative_ShouldThrowArgumentOutOfRangeException()
    {
        AzureServiceBusOptions options = new()
        {
            ConnectionString = TestData.ConnectionString,
            QueueName = "orders",
            MaxAutoLockRenewalDuration = TimeSpan.FromMilliseconds(-1)
        };

        Action action = () => options.Validate();

        action.Should().Throw<ArgumentOutOfRangeException>()
            .Which.ParamName.Should().Be(nameof(AzureServiceBusOptions.MaxAutoLockRenewalDuration));
    }

    #endregion

    #region Service Collection Tests

    [TestMethod]
    public async Task AddAzureServiceBusMessaging_WhenConfigurationDelegateIsNull_ShouldDeferValidationUntilResolution()
    {
        ServiceCollection services = [];

        services.AddAzureServiceBusMessaging();

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        Action action = () => _ = serviceProvider.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;

        action.Should().Throw<ArgumentException>()
            .Which.ParamName.Should().Be(nameof(AzureServiceBusOptions.ConnectionString));
    }

    [TestMethod]
    public async Task AddAzureServiceBusMessaging_WhenNamespaceAuthenticationIsConfigured_ShouldRegisterNamespaceClient()
    {
        ServiceCollection services = [];
        services.AddAzureServiceBusMessaging(options =>
        {
            options.FullyQualifiedNamespace = "contoso.servicebus.windows.net";
            options.TopicName = "orders";
            options.InitialRetryDelay = TimeSpan.Zero;
        });

        await using ServiceProvider serviceProvider = services.BuildServiceProvider();

        ServiceBusClient client = serviceProvider.GetRequiredService<ServiceBusClient>();
        AzureServiceBusOptions options = serviceProvider.GetRequiredService<IOptions<AzureServiceBusOptions>>().Value;

        client.FullyQualifiedNamespace.Should().Be("contoso.servicebus.windows.net");
        options.TopicName.Should().Be("orders");
    }

    #endregion

    #region Handler Registration Tests

    [TestMethod]
    public async Task StartAsync_WhenCalled_ShouldCreateAndStartConsumer()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageHandlerRegistration<RecordingTestHandler, TestMessage> registration = new(
            CreateBus(client),
            new RecordingTestHandler(),
            "billing");

        await registration.StartAsync(CancellationToken.None);

        client.QueueProcessor.StartCalls.Should().Be(1);
    }

    [TestMethod]
    public async Task StopAsync_WhenCalledBeforeStart_ShouldReturnWithoutError()
    {
        AzureServiceBusMessageHandlerRegistration<RecordingTestHandler, TestMessage> registration = new(
            CreateBus(new FakeServiceBusClient()),
            new RecordingTestHandler(),
            "billing");

        await registration.Awaiting(value => value.StopAsync(CancellationToken.None)).Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerImplementsAsyncDisposable_ShouldStopDisposeAndClearConsumer()
    {
        FakeServiceBusClient client = new();
        AzureServiceBusMessageHandlerRegistration<RecordingTestHandler, TestMessage> registration = new(
            CreateBus(client),
            new RecordingTestHandler(),
            "billing");

        await registration.StartAsync(CancellationToken.None);
        await registration.StopAsync(CancellationToken.None);

        client.QueueProcessor.StopCalls.Should().Be(1);
        client.QueueProcessor.DisposeCalls.Should().Be(1);
        GetRegistrationConsumer(registration).Should().BeNull();
    }

    [TestMethod]
    public async Task StopAsync_WhenConsumerImplementsDisposableOnly_ShouldDisposeAndClearConsumer()
    {
        AzureServiceBusMessageHandlerRegistration<RecordingTestHandler, TestMessage> registration = new(
            CreateBus(new FakeServiceBusClient()),
            new RecordingTestHandler(),
            "billing");
        DisposableOnlyConsumer consumer = new();
        SetRegistrationConsumer(registration, consumer);

        await registration.StopAsync(CancellationToken.None);

        consumer.StopCalls.Should().Be(1);
        consumer.DisposeCalls.Should().Be(1);
        GetRegistrationConsumer(registration).Should().BeNull();
    }

    #endregion

    #region Envelope Payload Tests

    [TestMethod]
    public void Create_WhenTypeMetadataLacksAssemblyQualifiedNameAndFullName_ShouldFallBackToTypeName()
    {
        MessageEnvelope envelope = MessageEnvelope.Create(new TestMessage(), correlationId: null);
        Type genericParameterType = new MissingTypeMetadataDelegator(typeof(TestMessage));

        Action action = () => _ = ServiceBusEnvelopePayload.Create(
            envelope,
            genericParameterType,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        action.Should().Throw<ArgumentException>();
    }

    #endregion

    private static AzureServiceBusMessageBus CreateBus(FakeServiceBusClient client, AzureServiceBusOptions? options = null)
    {
        return new AzureServiceBusMessageBus(
            client,
            Options.Create(options ?? TestData.CreateOptions()));
    }

    private static IMessageConsumer? GetRegistrationConsumer<THandler, TMessage>(
        AzureServiceBusMessageHandlerRegistration<THandler, TMessage> registration)
        where THandler : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        FieldInfo field = typeof(AzureServiceBusMessageHandlerRegistration<THandler, TMessage>)
            .GetField("consumer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (IMessageConsumer?)field.GetValue(registration);
    }

    private static void SetRegistrationConsumer<THandler, TMessage>(
        AzureServiceBusMessageHandlerRegistration<THandler, TMessage> registration,
        IMessageConsumer consumer)
        where THandler : IMessageHandler<TMessage>
        where TMessage : IMessage
    {
        FieldInfo field = typeof(AzureServiceBusMessageHandlerRegistration<THandler, TMessage>)
            .GetField("consumer", BindingFlags.Instance | BindingFlags.NonPublic)!;
        field.SetValue(registration, consumer);
    }

    private sealed class RecordingTestHandler : IMessageHandler<TestMessage>
    {
        public Task HandleAsync(TestMessage message, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class DisposableOnlyConsumer : IMessageConsumer, IDisposable
    {
        public int StopCalls { get; private set; }

        public int DisposeCalls { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StopCalls++;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            DisposeCalls++;
        }
    }

    private sealed class AsyncDisposingServiceBusClient : IAzureServiceBusClient
    {
        public AsyncDisposingServiceBusSender Sender { get; } = new();

        public IAzureServiceBusSender CreateSender(string entityPath)
        {
            return Sender;
        }

        public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }

        public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class NullSenderServiceBusClient : IAzureServiceBusClient
    {
        public IAzureServiceBusSender CreateSender(string entityPath)
        {
            return null!;
        }

        public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }

        public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class AsyncCompletingServiceBusClient : IAzureServiceBusClient
    {
        public AsyncCompletingServiceBusSender Sender { get; } = new();

        public IAzureServiceBusSender CreateSender(string entityPath)
        {
            return Sender;
        }

        public IAzureServiceBusProcessor CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }

        public IAzureServiceBusProcessor CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class AsyncDisposingServiceBusSender : IAzureServiceBusSender
    {
        public int DisposeCalls { get; private set; }

        public bool DisposeCompleted { get; private set; }

        public Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            DisposeCalls++;
            return new ValueTask(DisposeAsyncCore());
        }

        private async Task DisposeAsyncCore()
        {
            await Task.Yield();
            DisposeCompleted = true;
        }
    }

    private sealed class AsyncCompletingServiceBusSender : IAzureServiceBusSender
    {
        public int SendCalls { get; private set; }

        public int ScheduleCalls { get; private set; }

        public bool SendCompleted { get; private set; }

        public bool ScheduleCompleted { get; private set; }

        public async Task SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken)
        {
            SendCalls++;
            await Task.Yield();
            SendCompleted = true;
        }

        public async Task ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken)
        {
            ScheduleCalls++;
            await Task.Yield();
            ScheduleCompleted = true;
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }

    private sealed class MissingTypeMetadataDelegator(Type delegatingType) : TypeDelegator(delegatingType)
    {
        public override string? AssemblyQualifiedName => null;

        public override string? FullName => null;

        public override string Name => "TMessage";
    }

    private sealed record TestMessage : MessageBase
    {
        public string OrderNumber { get; init; } = "SO-0001";
    }
}
