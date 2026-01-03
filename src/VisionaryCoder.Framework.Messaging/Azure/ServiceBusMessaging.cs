// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VisionaryCoder.Framework.Messaging.Azure;

/// <summary>
/// Azure Service Bus implementation of IMessagePublisher.
/// </summary>
public sealed class ServiceBusMessagePublisher : IMessagePublisher
{
    private readonly ServiceBusClient client;
    private readonly ILogger<ServiceBusMessagePublisher> logger;
    private readonly JsonSerializerOptions jsonOptions;

    public ServiceBusMessagePublisher(
        ServiceBusClient client,
        ILogger<ServiceBusMessagePublisher> logger)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        string topicName = typeof(TMessage).Name;
        ServiceBusSender sender = client.CreateSender(topicName);

        try
        {
            string json = JsonSerializer.Serialize(message, jsonOptions);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                ContentType = "application/json"
            };

            await sender.SendMessageAsync(serviceBusMessage, cancellationToken);

            logger.LogInformation(
                "Published message {MessageId} of type {MessageType} to topic {TopicName}",
                message.MessageId,
                typeof(TMessage).Name,
                topicName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish message {MessageId} of type {MessageType}",
                message.MessageId,
                typeof(TMessage).Name);
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async Task PublishBatchAsync<TMessage>(IEnumerable<TMessage> messages, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(messages);

        var messageList = messages.ToList();
        if (messageList.Count == 0)
        {
            return;
        }

        string topicName = typeof(TMessage).Name;
        ServiceBusSender sender = client.CreateSender(topicName);

        try
        {
            using ServiceBusMessageBatch batch = await sender.CreateMessageBatchAsync(cancellationToken);

            foreach (TMessage message in messageList)
            {
                string json = JsonSerializer.Serialize(message, jsonOptions);
                var serviceBusMessage = new ServiceBusMessage(json)
                {
                    MessageId = message.MessageId,
                    CorrelationId = message.CorrelationId,
                    ContentType = "application/json"
                };

                if (!batch.TryAddMessage(serviceBusMessage))
                {
                    // Batch is full, send it and create a new one
                    await sender.SendMessagesAsync(batch, cancellationToken);
                    batch.TryAddMessage(serviceBusMessage);
                }
            }

            if (batch.Count > 0)
            {
                await sender.SendMessagesAsync(batch, cancellationToken);
            }

            logger.LogInformation(
                "Published batch of {Count} messages of type {MessageType} to topic {TopicName}",
                messageList.Count,
                typeof(TMessage).Name,
                topicName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to publish batch of messages of type {MessageType}",
                typeof(TMessage).Name);
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async Task ScheduleAsync<TMessage>(TMessage message, DateTimeOffset scheduledTime, CancellationToken cancellationToken = default)
        where TMessage : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        string topicName = typeof(TMessage).Name;
        ServiceBusSender sender = client.CreateSender(topicName);

        try
        {
            string json = JsonSerializer.Serialize(message, jsonOptions);
            var serviceBusMessage = new ServiceBusMessage(json)
            {
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                ContentType = "application/json"
            };

            await sender.ScheduleMessageAsync(serviceBusMessage, scheduledTime, cancellationToken);

            logger.LogInformation(
                "Scheduled message {MessageId} of type {MessageType} for {ScheduledTime}",
                message.MessageId,
                typeof(TMessage).Name,
                scheduledTime);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to schedule message {MessageId} of type {MessageType}",
                message.MessageId,
                typeof(TMessage).Name);
            throw;
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }
}

/// <summary>
/// Azure Service Bus implementation of IMessageConsumer.
/// </summary>
public sealed class ServiceBusMessageConsumer<TMessage> : IMessageConsumer
    where TMessage : IMessage
{
    private readonly ServiceBusClient client;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ServiceBusMessageConsumer<TMessage>> logger;
    private readonly string subscriptionName;
    private readonly JsonSerializerOptions jsonOptions;
    private ServiceBusProcessor? processor;

    public ServiceBusMessageConsumer(
        ServiceBusClient client,
        IServiceProvider serviceProvider,
        ILogger<ServiceBusMessageConsumer<TMessage>> logger,
        string subscriptionName)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.subscriptionName = subscriptionName ?? throw new ArgumentNullException(nameof(subscriptionName));

        jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        string topicName = typeof(TMessage).Name;
        processor = client.CreateProcessor(topicName, subscriptionName);

        processor.ProcessMessageAsync += ProcessMessageAsync;
        processor.ProcessErrorAsync += ProcessErrorAsync;

        await processor.StartProcessingAsync(cancellationToken);

        logger.LogInformation(
            "Started consuming messages of type {MessageType} from subscription {SubscriptionName}",
            typeof(TMessage).Name,
            subscriptionName);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (processor != null)
        {
            await processor.StopProcessingAsync(cancellationToken);
            await processor.DisposeAsync();

            logger.LogInformation(
                "Stopped consuming messages of type {MessageType} from subscription {SubscriptionName}",
                typeof(TMessage).Name,
                subscriptionName);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            string json = args.Message.Body.ToString();
            TMessage? message = JsonSerializer.Deserialize<TMessage>(json, jsonOptions);

            if (message == null)
            {
                logger.LogWarning("Failed to deserialize message {MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "Deserialization failed");
                return;
            }

            using IServiceScope scope = serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();

            await handler.HandleAsync(message, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            logger.LogInformation(
                "Processed message {MessageId} of type {MessageType}",
                args.Message.MessageId,
                typeof(TMessage).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing message {MessageId} of type {MessageType}",
                args.Message.MessageId,
                typeof(TMessage).Name);

            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception,
            "Error in Service Bus processor for {MessageType}: {ErrorSource}",
            typeof(TMessage).Name,
            args.ErrorSource);

        return Task.CompletedTask;
    }
}
