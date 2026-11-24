---
title: Component Surface Area
description: Inventory of public component methods and their stated intent.
doc_type: reference
status: active
last_updated: 2026-11-11
---

# Component Surface Area

## vc.Ifx
### Ifx.Integrations.Archiving.SevenZip
| Method | Intent |
|--------|--------|
| AddSevenZipArchiving(this IServiceCollection services) : static IServiceCollection | AddSevenZipArchiving |
| CanRead(string archivePath) : bool | CanRead |
| ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default) : async Task<ArchiveExtractionResult> | ExtractAsync |
| ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default) : async Task<IReadOnlyList<ArchiveEntry>> | ListEntriesAsync |

### Ifx.Integrations.Azure.Configuration
| Method | Intent |
|--------|--------|
| AddAzureAppConfiguration(this IServiceCollection services, IConfiguration configuration, Action<ConfigurationOptions>? configure = null) : static IServiceCollection | AddAzureAppConfiguration |
| AddAzureAppConfiguration(this IConfigurationBuilder builder, ConfigurationOptions options) : static IConfigurationBuilder | AddAzureAppConfiguration |

### Ifx.Integrations.Azure.Configuration.Azure
| Method | Intent |
|--------|--------|
| Refresh() : override bool | Refresh |
| RefreshAsync(CancellationToken cancellationToken = default) : override async Task<bool> | RefreshAsync |
| Validate(this AzureConfigurationProviderOptions options) : static void | Validate |
| ApplyTo(this AzureConfigurationProviderOptions options, AzureAppConfigurationOptions clientOptions) : static AzureAppConfigurationOptions | ApplyTo |

### Ifx.Integrations.Azure.Messaging.Queues
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |
| CreateClientOptions() : QueueClientOptions | CreateClientOptions |
| QueueExists() : bool | QueueExists |
| QueueExistsAsync(CancellationToken cancellationToken = default) : async Task<bool> | QueueExistsAsync |
| SendMessage(string messageText) : void | SendMessage |
| SendMessageAsync(string messageText, CancellationToken cancellationToken = default) : async Task | SendMessageAsync |
| ReceiveMessages(int? maxMessages = null) : QueueMessage[] | ReceiveMessages |
| ReceiveMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default) : async Task<QueueMessage[]> | ReceiveMessagesAsync |
| PeekMessages(int? maxMessages = null) : PeekedMessage[] | PeekMessages |
| PeekMessagesAsync(int? maxMessages = null, CancellationToken cancellationToken = default) : async Task<PeekedMessage[]> | PeekMessagesAsync |
| DeleteMessage(string messageId, string popReceipt) : void | DeleteMessage |
| DeleteMessageAsync(string messageId, string popReceipt, CancellationToken cancellationToken = default) : async Task | DeleteMessageAsync |
| UpdateMessage(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null) : void | UpdateMessage |
| UpdateMessageAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) : async Task | UpdateMessageAsync |
| UpdateMessageWithReceipt(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null) : UpdateReceipt | UpdateMessageWithReceipt |
| UpdateMessageWithReceiptAsync(string messageId, string popReceipt, string? messageText = null, TimeSpan? visibilityTimeout = null, CancellationToken cancellationToken = default) : async Task<UpdateReceipt> | UpdateMessageWithReceiptAsync |
| GetMessageCount() : int | GetMessageCount |
| GetMessageCountAsync(CancellationToken cancellationToken = default) : async Task<int> | GetMessageCountAsync |
| ClearMessages() : void | ClearMessages |
| ClearMessagesAsync(CancellationToken cancellationToken = default) : async Task | ClearMessagesAsync |
| Dispose() : void | Dispose |

### Ifx.Integrations.Azure.Messaging.Queues.Registration
| Method | Intent |
|--------|--------|
| AddAzureQueueStorage(this IServiceCollection services, AzureQueueStorageOptions options) : static IServiceCollection | AddAzureQueueStorage |
| AddNamedAzureQueueStorage(this IServiceCollection services, string name, AzureQueueStorageOptions options) : static IServiceCollection | AddNamedAzureQueueStorage |
| AddQueue(this StorageRegistrationBuilder builder, string name, AzureQueueStorageOptions options) : static StorageRegistrationBuilder | AddQueue |

### Ifx.Integrations.Azure.Messaging.ServiceBus
| Method | Intent |
|--------|--------|
| CreateSender(string entityPath) : IAzureServiceBusSender | CreateSender |
| CreateQueueProcessor(string queueName, ServiceBusProcessorOptions options) : IAzureServiceBusProcessor | CreateQueueProcessor |
| CreateTopicProcessor(string topicName, string subscriptionName, ServiceBusProcessorOptions options) : IAzureServiceBusProcessor | CreateTopicProcessor |
| HandleAsync(TMessage message, CancellationToken cancellationToken = default) : Task | HandleAsync |
| StartAsync(CancellationToken cancellationToken = default) : Task | StartAsync |
| StopAsync(CancellationToken cancellationToken = default) : async Task | StopAsync |
| DisposeAsync() : async ValueTask | DisposeAsync |
| StartAsync(CancellationToken cancellationToken) : async Task | StartAsync |
| StopAsync(CancellationToken cancellationToken) : async Task | StopAsync |
| Validate() : AzureServiceBusOptions | Validate |
| StartProcessingAsync(CancellationToken cancellationToken) : Task | StartProcessingAsync |
| StopProcessingAsync(CancellationToken cancellationToken) : Task | StopProcessingAsync |
| DisposeAsync() : async ValueTask | DisposeAsync |
| CompleteMessageAsync(CancellationToken cancellationToken) : Task | CompleteMessageAsync |
| AbandonMessageAsync(CancellationToken cancellationToken) : Task | AbandonMessageAsync |
| DeadLetterMessageAsync(string reason, string? description, CancellationToken cancellationToken) : Task | DeadLetterMessageAsync |
| SendMessageAsync(ServiceBusMessage message, CancellationToken cancellationToken) : async Task | SendMessageAsync |
| ScheduleMessageAsync(ServiceBusMessage message, DateTimeOffset scheduledEnqueueTime, CancellationToken cancellationToken) : async Task | ScheduleMessageAsync |
| DisposeAsync() : async ValueTask | DisposeAsync |
| Create(MessageEnvelope envelope, Type messageType, JsonSerializerOptions serializerOptions) : static ServiceBusEnvelopePayload | Create |
| AddAzureServiceBusMessaging(
        this IServiceCollection services,
        Action<AzureServiceBusOptions>? configureOptions = null) : static IServiceCollection | AddAzureServiceBusMessaging |

### Ifx.Integrations.Azure.Secrets.KeyVault
| Method | Intent |
|--------|--------|
| AddAzureKeyVaultSecrets(this IServiceCollection services,
        IConfiguration configuration, Action<KeyVaultOptions>? configure = null) : static IServiceCollection | AddAzureKeyVaultSecrets |
| AddNullSecrets(this IServiceCollection services) : static IServiceCollection | AddNullSecrets |
| GetAsync(string name, CancellationToken cancellationToken = default) : Task<string?> | GetAsync |
| GetAsync(string name, string? version, CancellationToken cancellationToken) : async Task<string?> | GetAsync |

### Ifx.Integrations.Azure.Storage.Blobs
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |
| OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : async Task<Stream> | OpenReadAsync |
| Dispose() : void | Dispose |
| WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default) : async Task<StorageObjectMetadata> | WriteAsync |
| GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : async Task<StorageObjectMetadata?> | GetMetadataAsync |
| DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : async Task | DeleteAsync |
| ListAsync(StorageListRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<StorageObjectMetadata> | ListAsync |
| FileExists(FileInfo fileInfo) : bool | FileExists |
| FileExists(string path) : bool | FileExists |
| ReadAllText(string path) : string | ReadAllText |
| ReadAllTextAsync(string path, CancellationToken cancellationToken = default) : async Task<string> | ReadAllTextAsync |
| ReadAllBytes(string path) : byte[] | ReadAllBytes |
| ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) : async Task<byte[]> | ReadAllBytesAsync |
| WriteAllText(string path, string content) : void | WriteAllText |
| WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) : Task | WriteAllTextAsync |
| WriteAllBytes(string path, byte[] bytes) : void | WriteAllBytes |
| WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) : async Task | WriteAllBytesAsync |
| DeleteFile(string path) : void | DeleteFile |
| DeleteFileAsync(string path, CancellationToken cancellationToken = default) : Task | DeleteFileAsync |
| DirectoryExists(string path) : bool | DirectoryExists |
| CreateDirectory(string path) : DirectoryInfo | CreateDirectory |
| CreateDirectoryAsync(string path, CancellationToken cancellationToken = default) : async Task<DirectoryInfo> | CreateDirectoryAsync |
| DeleteDirectory(string path, bool recursive = true) : void | DeleteDirectory |
| DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default) : async Task | DeleteDirectoryAsync |
| GetFiles(string path, string searchPattern = "*") : string[] | GetFiles |
| GetDirectories(string path, string searchPattern = "*") : string[] | GetDirectories |
| EnumerateFilesAsync(string path, string searchPattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| GetFullPath(string path) : string | GetFullPath |
| GetDirectoryName(string path) : string? | GetDirectoryName |
| GetFileName(string path) : string | GetFileName |

### Ifx.Integrations.Azure.Storage.Blobs.Registration
| Method | Intent |
|--------|--------|
| AddAzureBlobStorage(this IServiceCollection services, AzureBlobStorageOptions options) : static IServiceCollection | AddAzureBlobStorage |
| AddNamedAzureBlobStorage(this IServiceCollection services, string name, AzureBlobStorageOptions options) : static IServiceCollection | AddNamedAzureBlobStorage |
| AddBlob(this StorageRegistrationBuilder builder, string name, AzureBlobStorageOptions options) : static StorageRegistrationBuilder | AddBlob |

### Ifx.Integrations.Azure.Storage.Tables
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |
| CreateClientOptions() : TableClientOptions | CreateClientOptions |
| TableExists() : bool | TableExists |
| TableExistsAsync(CancellationToken cancellationToken = default) : async Task<bool> | TableExistsAsync |
| DeleteEntity(string partitionKey, string rowKey, ETag etag = default) : void | DeleteEntity |
| DeleteEntityAsync(string partitionKey, string rowKey, ETag etag = default, CancellationToken cancellationToken = default) : async Task | DeleteEntityAsync |
| SubmitBatch(IEnumerable<TableTransactionAction> actions) : void | SubmitBatch |
| SubmitBatchAsync(IEnumerable<TableTransactionAction> actions, CancellationToken cancellationToken = default) : async Task | SubmitBatchAsync |

### Ifx.Integrations.Azure.Storage.Tables.Registration
| Method | Intent |
|--------|--------|
| AddAzureTableStorage(this IServiceCollection services, AzureTableStorageOptions options) : static IServiceCollection | AddAzureTableStorage |
| AddNamedAzureTableStorage(this IServiceCollection services, string name, AzureTableStorageOptions options) : static IServiceCollection | AddNamedAzureTableStorage |
| AddTable(this StorageRegistrationBuilder builder, string name, AzureTableStorageOptions options) : static StorageRegistrationBuilder | AddTable |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Interceptors
| Method | Intent |
|--------|--------|
| AddProxyInterceptors(
        this IServiceCollection services,
        Action<ProxyOptions>? configureOptions = null) : static IServiceCollection | AddProxyInterceptors |
| AddSecurityInterceptor(this IServiceCollection services) : static IServiceCollection | AddSecurityInterceptor |
| AddJwtBearerEnricher(
        this IServiceCollection services,
        Func<IServiceProvider, Task<string?>> tokenProvider) : static IServiceCollection | AddJwtBearerEnricher |
| AddTelemetryInterceptor(this IServiceCollection services) : static IServiceCollection | AddTelemetryInterceptor |
| AddCorrelationInterceptor(this IServiceCollection services) : static IServiceCollection | AddCorrelationInterceptor |
| AddLoggingInterceptor(this IServiceCollection services) : static IServiceCollection | AddLoggingInterceptor |
| AddCachingInterceptor(this IServiceCollection services) : static IServiceCollection | AddCachingInterceptor |
| AddResilienceInterceptor(this IServiceCollection services) : static IServiceCollection | AddResilienceInterceptor |
| AddRetryInterceptor(this IServiceCollection services) : static IServiceCollection | AddRetryInterceptor |
| AddAuditingInterceptor(this IServiceCollection services) : static IServiceCollection | AddAuditingInterceptor |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Interceptors.Authentication
| Method | Intent |
|--------|--------|
| AddJwtAuthentication(this IServiceCollection services, Action<JwtOptions> configureOptions) : static IServiceCollection | AddJwtAuthentication |
| UseDefaultAuthenticationProviders(this IServiceCollection services) : static IServiceCollection | UseDefaultAuthenticationProviders |
| AddKeyVaultJwtAuthentication(this IServiceCollection services, Action<KeyVaultJwtOptions> configureOptions) : static IServiceCollection | AddKeyVaultJwtAuthentication |
| AddUserContext(this IServiceCollection services) : static IServiceCollection | AddUserContext |
| AddTenantContext(this IServiceCollection services) : static IServiceCollection | AddTenantContext |
| AddAuthenticationInterceptors(this IServiceCollection services) : static IServiceCollection | AddAuthenticationInterceptors |
| AddCompleteAuthentication(this IServiceCollection services, Action<JwtOptions> configureJwtOptions) : static IServiceCollection | AddCompleteAuthentication |
| AddAuthenticationWithValidation(this IServiceCollection services,
        Action<JwtOptions> configureOptions, bool validateSetup = true) : static IServiceCollection | AddAuthenticationWithValidation |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Interceptors.Authentication.Providers
| Method | Intent |
|--------|--------|
| GetTenantId() : string? | GetTenantId |
| GetTenantIdAsync(CancellationToken cancellationToken = default) : async Task<string?> | GetTenantIdAsync |
| GetTenantContextAsync(CancellationToken cancellationToken = default) : async Task<TenantContext?> | GetTenantContextAsync |
| GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default) : async Task<TenantContext?> | GetTenantContextAsync |
| ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default) : async Task<bool> | ValidateTenantContextAsync |
| GetCurrentTenantAsync(CancellationToken cancellationToken = default) : async Task<TenantContext> | GetCurrentTenantAsync |
| IsTenantValid(string tenantId) : bool | IsTenantValid |
| SwitchTenant(string tenantId) : bool | SwitchTenant |
| GetUserAsync(string userId, CancellationToken cancellationToken = default) : async Task<UserContext?> | GetUserAsync |
| ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default) : async Task<bool> | ValidateUserContextAsync |
| GetCurrentUserAsync(CancellationToken cancellationToken = default) : async Task<UserContext?> | GetCurrentUserAsync |
| HasPermission(string permission) : bool | HasPermission |
| IsInRole(string role) : bool | IsInRole |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Interceptors.Security
| Method | Intent |
|--------|--------|
| AddJwtBearerInterceptor(this IServiceCollection services,
        Func<CancellationToken, Task<string?>> tokenProvider) : static IServiceCollection | AddJwtBearerInterceptor |
| AddJwtBearerInterceptorFromSecret(this IServiceCollection services, string secretName) : static IServiceCollection | AddJwtBearerInterceptorFromSecret |
| AddJwtBearerInterceptorWithStaticToken(this IServiceCollection services, string staticToken) : static IServiceCollection | AddJwtBearerInterceptorWithStaticToken |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Interceptors.Security.Web
| Method | Intent |
|--------|--------|
| EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default) : async Task | EnrichAsync |

### Ifx.Integrations.Hosting.AspNetCore.Proxies.Primitives
| Method | Intent |
|--------|--------|
| BindModelAsync(ModelBindingContext bindingContext) : Task | BindModelAsync |
| GetBinder(ModelBinderProviderContext context) : IModelBinder? | GetBinder |

### Ifx.Integrations.Hosting.AspNetCore.WebApi.DependencyInjection
| Method | Intent |
|--------|--------|
| AddIfxWebApi(
        this IServiceCollection services,
        Action<WebApiExceptionHandlingOptions>? configureExceptionHandling = null,
        Action<WebApiResilienceOptions>? configureResilience = null) : static IServiceCollection | AddIfxWebApi |
| UseIfxWebApiExceptionHandling(this IApplicationBuilder app) : static IApplicationBuilder | UseIfxWebApiExceptionHandling |

### Ifx.Integrations.Hosting.AspNetCore.WebApi.ExceptionHandling
| Method | Intent |
|--------|--------|
| GlobalExceptionHandler(
    IProblemDetailsExceptionMapper exceptionMapper,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : sealed class | GlobalExceptionHandler |
| TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken) : async ValueTask<bool> | TryHandleAsync |
| ProblemDetailsExceptionMapper(IOptions<WebApiExceptionHandlingOptions> options) : sealed class | ProblemDetailsExceptionMapper |
| Map(HttpContext httpContext, Exception exception) : ProblemDetails | Map |
| ProblemDetailsExceptionMapping(int StatusCode, string Title, string? Type = null) : sealed record | ProblemDetailsExceptionMapping |

### Ifx.Integrations.Hosting.AspNetCore.WebApi.Resilience
| Method | Intent |
|--------|--------|
| WebApiResiliencePipelineFactory(
    IOptions<WebApiResilienceOptions> options,
    IWebApiTransientFailureClassifier? classifier = null,
    TimeProvider? timeProvider = null) : sealed class | WebApiResiliencePipelineFactory |
| CreatePipeline() : ResiliencePipeline | CreatePipeline |
| IsTransient(Exception exception) : bool | IsTransient |

### Ifx.Integrations.Hosting.AspNetCore.WebApi.Responses
| Method | Intent |
|--------|--------|
| Get(int statusCode) : static HttpResponseDefinition | Get |
| TryGet(int statusCode, [NotNullWhen(true) : static bool | TryGet |
| CanWriteBody(int statusCode, string method) : static bool | CanWriteBody |

### Ifx.Integrations.Messaging
| Method | Intent |
|--------|--------|
| Create(IMessage message, Guid? correlationId = null) : static MessageEnvelope | Create |
| ValidateNotNullOrWhiteSpace(this string? value, string optionName) : static string | ValidateNotNullOrWhiteSpace |
| ExecuteAsync(
        Func<Task> operation,
        int maxAttempts,
        TimeSpan initialDelay,
        CancellationToken cancellationToken = default) : static Task | ExecuteAsync |
| ExecuteAsync(
        Func<CancellationToken, Task> operation,
        int maxAttempts,
        TimeSpan initialDelay,
        CancellationToken cancellationToken = default) : static async Task | ExecuteAsync |

### Ifx.Integrations.Messaging.RabbitMQ
| Method | Intent |
|--------|--------|
| DeclareExchangeAsync(
        string exchangeName,
        string exchangeType,
        CancellationToken cancellationToken = default) : Task | DeclareExchangeAsync |
| DeclareConsumerTopologyAsync(
        RabbitMqTopology topology,
        RabbitMqOptions options,
        CancellationToken cancellationToken = default) : async Task | DeclareConsumerTopologyAsync |
| PublishAsync(
        string exchangeName,
        string routingKey,
        string messageType,
        string messageId,
        string? correlationId,
        DateTimeOffset enqueuedAtUtc,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default) : Task | PublishAsync |
| StartConsumerAsync(
        string queueName,
        Func<RabbitMqDelivery, CancellationToken, Task> onReceived,
        CancellationToken cancellationToken = default) : Task<string> | StartConsumerAsync |
| CancelConsumerAsync(string consumerTag, CancellationToken cancellationToken = default) : Task | CancelConsumerAsync |
| AcknowledgeAsync(ulong deliveryTag, CancellationToken cancellationToken = default) : Task | AcknowledgeAsync |
| RejectAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default) : Task | RejectAsync |
| NegativeAcknowledgeAsync(ulong deliveryTag, bool requeue, CancellationToken cancellationToken = default) : Task | NegativeAcknowledgeAsync |
| Dispose() : void | Dispose |
| DisposeAsync() : ValueTask | DisposeAsync |
| Dispose() : void | Dispose |
| DisposeAsync() : async ValueTask | DisposeAsync |
| HandleAsync(TMessage message, CancellationToken cancellationToken = default) : Task | HandleAsync |
| StartAsync(CancellationToken cancellationToken = default) : async Task | StartAsync |
| StopAsync(CancellationToken cancellationToken = default) : async Task | StopAsync |
| Dispose() : void | Dispose |
| StartAsync(CancellationToken cancellationToken) : async Task | StartAsync |
| StopAsync(CancellationToken cancellationToken) : async Task | StopAsync |
| Validate() : RabbitMqOptions | Validate |
| AddRabbitMqMessaging(
        this IServiceCollection services,
        Action<RabbitMqOptions> configure) : static IServiceCollection | AddRabbitMqMessaging |
| AddRabbitMqMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionPath = RabbitMqOptions.DefaultSectionName) : static IServiceCollection | AddRabbitMqMessaging |

### Ifx.Integrations.Proxy.Interceptors
| Method | Intent |
|--------|--------|
| AuthInterceptor(IAuthorizationService auth) : sealed class | AuthInterceptor |
| DefaultPolicy() : static AsyncPolicy | DefaultPolicy |
| DefaultPipeline(bool operationsAreReplaySafe = false, TimeProvider? timeProvider = null) : static ResiliencePipeline | DefaultPipeline |
| TracingInterceptor(ITracer tracer) : sealed class | TracingInterceptor |

### Ifx.Integrations.Proxy.Interceptors.Auditing
| Method | Intent |
|--------|--------|
| AuditingInterceptor(ILogger<AuditingInterceptor> logger, IEnumerable<IAuditSink> auditSinks) : sealed class | AuditingInterceptor |
| LoggingAuditSink(ILogger<LoggingAuditSink> logger) : sealed class | LoggingAuditSink |
| WriteAsync(AuditRecord auditRecord, CancellationToken cancellationToken = default) : Task | WriteAsync |
| NullAuditingInterceptor(int order = 300) : sealed class | NullAuditingInterceptor |

### Ifx.Integrations.Proxy.Interceptors.Authentication
| Method | Intent |
|--------|--------|
| HasFeature(string featureName) : bool | HasFeature |
| GetResourceLimit(string resourceName) : int? | GetResourceLimit |
| HasRole(string roleName) : bool | HasRole |
| HasPermission(string permission) : bool | HasPermission |
| HasAnyRole(params string[] roleNames) : bool | HasAnyRole |
| HasAllRoles(params string[] roleNames) : bool | HasAllRoles |

### Ifx.Integrations.Proxy.Interceptors.Authentication.Interceptors
| Method | Intent |
|--------|--------|
| IsValid() : bool | IsValid |

### Ifx.Integrations.Proxy.Interceptors.Authentication.Jwt
| Method | Intent |
|--------|--------|
| IsValid() : bool | IsValid |
| GetScopeString() : string? | GetScopeString |
| CreateForWebApp(string authority, string audience, string clientId, string clientSecret) : static JwtOptions | CreateForWebApp |
| CreateForApiClient(string authority, string audience, params string[] scopes) : static JwtOptions | CreateForApiClient |
| IsValid() : bool | IsValid |
| GetScopeString() : string? | GetScopeString |
| CreateClientCredentials(string clientId, string clientSecret, string[]? scopes = null, string? audience = null) : static TokenRequest | CreateClientCredentials |
| CreatePasswordCredentials(string clientId, string username, string password, string[]? scopes = null) : static TokenRequest | CreatePasswordCredentials |
| IsCloseToExpiry(TimeSpan? threshold = null) : bool | IsCloseToExpiry |
| UpdateExpiryTime() : void | UpdateExpiryTime |
| Success(string accessToken, int expiresIn, string? refreshToken = null, string? scope = null) : static TokenResult | Success |
| Failure(string error, string? errorDescription = null, string? errorUri = null) : static TokenResult | Failure |
| ToString() : override string | ToString |
| ToJson() : string | ToJson |

### Ifx.Integrations.Proxy.Interceptors.Authentication.Providers
| Method | Intent |
|--------|--------|
| GetTokenAsync(CancellationToken cancellationToken = default) : async Task<string> | GetTokenAsync |
| GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default) : async Task<TokenResult> | GetTokenAsync |
| ValidateTokenAsync(string token, CancellationToken cancellationToken = default) : Task<bool> | ValidateTokenAsync |
| ValidateToken(string token) : bool | ValidateToken |
| RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) : async Task<TokenResult?> | RefreshTokenAsync |
| GetTenantId() : string? | GetTenantId |
| GetTenantIdAsync(CancellationToken cancellationToken = default) : Task<string?> | GetTenantIdAsync |
| GetTenantContextAsync(CancellationToken cancellationToken = default) : Task<TenantContext?> | GetTenantContextAsync |
| GetTenantContextAsync(string tenantId, CancellationToken cancellationToken = default) : Task<TenantContext?> | GetTenantContextAsync |
| ValidateTenantContextAsync(TenantContext tenantContext, CancellationToken cancellationToken = default) : Task<bool> | ValidateTenantContextAsync |
| GetTokenAsync(CancellationToken cancellationToken = default) : Task<string> | GetTokenAsync |
| GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default) : Task<TokenResult> | GetTokenAsync |
| ValidateTokenAsync(string token, CancellationToken cancellationToken = default) : Task<bool> | ValidateTokenAsync |
| ValidateToken(string token) : bool | ValidateToken |
| RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default) : Task<TokenResult?> | RefreshTokenAsync |
| GetCurrentUserAsync(CancellationToken cancellationToken = default) : Task<UserContext?> | GetCurrentUserAsync |
| GetUserAsync(string userId, CancellationToken cancellationToken = default) : Task<UserContext?> | GetUserAsync |
| ValidateUserContextAsync(UserContext userContext, CancellationToken cancellationToken = default) : Task<bool> | ValidateUserContextAsync |

### Ifx.Integrations.Proxy.Interceptors.Authorization
| Method | Intent |
|--------|--------|
| AddAuthorization(this IServiceCollection services) : static IServiceCollection | AddAuthorization |
| AddRoleBasedAuthorization(this IServiceCollection services, params string[] requiredRoles) : static IServiceCollection | AddRoleBasedAuthorization |
| AddAuthorizationPolicy(this IServiceCollection services, IAuthorizationPolicy policy) : static IServiceCollection | AddAuthorizationPolicy |
| UseDefaultRoleBasedAuthorization(this IServiceCollection services, params string[] defaultRoles) : static IServiceCollection | UseDefaultRoleBasedAuthorization |
| IsAuthorizedAsync(ProxyContext context, CancellationToken cancellationToken = default) : async Task<bool> | IsAuthorizedAsync |

### Ifx.Integrations.Proxy.Interceptors.Authorization.Policies
| Method | Intent |
|--------|--------|
| Evaluate(object context) : bool | Evaluate |
| EvaluateAsync(object context, CancellationToken cancellationToken = default) : Task<AuthorizationResult> | EvaluateAsync |
| Evaluate(object context) : bool | Evaluate |
| EvaluateAsync(object context, CancellationToken cancellationToken = default) : Task<AuthorizationResult> | EvaluateAsync |

### Ifx.Integrations.Proxy.Interceptors.Authorization.Results
| Method | Intent |
|--------|--------|
| Success() : static AuthorizationResult | Success |
| Failure(string reason) : static AuthorizationResult | Failure |

### Ifx.Integrations.Proxy.Interceptors.Caching
| Method | Intent |
|--------|--------|
| AddCaching(this IServiceCollection services, Action<CachingOptions>? configure = null) : static IServiceCollection | AddCaching |
| AddCaching(this IServiceCollection services, TimeSpan defaultDuration, int? maxCacheSize = null, bool enableEvictionLogging = false) : static IServiceCollection | AddCaching |
| AddDistributedCaching(this IServiceCollection services, Action<CachingOptions> configure) : static IServiceCollection | AddDistributedCaching |
| UseDefaultCachingProviders(this IServiceCollection services) : static IServiceCollection | UseDefaultCachingProviders |
| GenerateKey(ProxyContext context) : string? | GenerateKey |
| AddCachingInterceptor(
        this IServiceCollection services,
        Action<CachingOptions>? configure = null) : static IServiceCollection | AddCachingInterceptor |
| AddCachingInterceptor(
        this IServiceCollection services,
        TimeSpan defaultCacheDuration,
        Func<ProxyContext, string>? keyGenerator = null) : static IServiceCollection | AddCachingInterceptor |
| MemoryProxyCache(IMemoryCache cache, ILogger<MemoryProxyCache>? logger = null) : sealed class | MemoryProxyCache |
| RemoveAsync(string key, CancellationToken cancellationToken = default) : Task | RemoveAsync |
| ClearAsync(CancellationToken cancellationToken = default) : Task | ClearAsync |
| ExistsAsync(string key, CancellationToken cancellationToken = default) : Task<bool> | ExistsAsync |

### Ifx.Integrations.Proxy.Interceptors.Caching.Providers
| Method | Intent |
|--------|--------|
| GenerateKey(ProxyContext context) : string | GenerateKey |
| DefaultCachePolicyProvider(CachingOptions options) : class | DefaultCachePolicyProvider |
| GetPolicy(ProxyContext context) : CachePolicy | GetPolicy |
| ShouldCache(ProxyContext context) : bool | ShouldCache |
| GetExpiration(ProxyContext context) : TimeSpan? | GetExpiration |
| GenerateKey(ProxyContext context) : string? | GenerateKey |
| CanGenerateKey(ProxyContext context) : bool | CanGenerateKey |
| GetPolicy(ProxyContext context) : CachePolicy | GetPolicy |
| GetExpiration(ProxyContext context) : TimeSpan? | GetExpiration |
| ShouldCache(ProxyContext context) : bool | ShouldCache |
| RemoveAsync(string key, CancellationToken cancellationToken = default) : Task | RemoveAsync |
| ClearAsync(CancellationToken cancellationToken = default) : Task | ClearAsync |
| ExistsAsync(string key, CancellationToken cancellationToken = default) : Task<bool> | ExistsAsync |

### Ifx.Integrations.Proxy.Interceptors.Configuration
| Method | Intent |
|--------|--------|
| Refresh() : virtual bool | Refresh |
| RefreshAsync(CancellationToken cancellationToken = default) : abstract Task<bool> | RefreshAsync |
| Validate(this ConfigurationProviderOptions options) : static void | Validate |

### Ifx.Integrations.Proxy.Interceptors.Configuration.Local
| Method | Intent |
|--------|--------|
| Refresh() : override bool | Refresh |
| RefreshAsync(CancellationToken cancellationToken = default) : override async Task<bool> | RefreshAsync |
| Validate(this LocalConfigurationProviderOptions options) : static void | Validate |

### Ifx.Integrations.Proxy.Interceptors.Correlation
| Method | Intent |
|--------|--------|
| SetCorrelationId(string correlationId) : void | SetCorrelationId |
| GenerateId() : string | GenerateId |

### Ifx.Integrations.Proxy.Interceptors.Logging
| Method | Intent |
|--------|--------|
| AddLogging(this IServiceCollection services) : static IServiceCollection | AddLogging |
| AddLoggingInterceptor(this IServiceCollection services) : static IServiceCollection | AddLoggingInterceptor |
| AddTimingInterceptor(this IServiceCollection services, long slowThresholdMs = 1000, long criticalThresholdMs = 5000) : static IServiceCollection | AddTimingInterceptor |
| AddNullLogging(this IServiceCollection services) : static IServiceCollection | AddNullLogging |
| AddLogging(this IServiceCollection services, Action<LoggingOptions> configureOptions) : static IServiceCollection | AddLogging |
| UseLoggingInterceptor(this IServiceCollection services) : static IServiceCollection | UseLoggingInterceptor |
| UseTimingInterceptor(
        this IServiceCollection services,
        long slowThresholdMs = 1000,
        long criticalThresholdMs = 5000) : static IServiceCollection | UseTimingInterceptor |
| UseDefaultLoggingInterceptors(this IServiceCollection services) : static IServiceCollection | UseDefaultLoggingInterceptors |
| LoggingInterceptor(ILogger<LoggingInterceptor> logger) : sealed class | LoggingInterceptor |
| AddLoggingInterceptor(this IServiceCollection services) : static IServiceCollection | AddLoggingInterceptor |
| TimingInterceptor(ILogger<TimingInterceptor> logger) : sealed class | TimingInterceptor |

### Ifx.Integrations.Proxy.Interceptors.Resilience
| Method | Intent |
|--------|--------|
| Dispose() : void | Dispose |
| ResilienceInterceptor(ILogger<ResilienceInterceptor> logger, ResiliencePipeline? resiliencePipeline = null) : sealed class | ResilienceInterceptor |

### Ifx.Integrations.Proxy.Interceptors.Security
| Method | Intent |
|--------|--------|
| TenantContextEnricher(ITenantContextProvider tenantProvider) : class | TenantContextEnricher |
| EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default) : async Task | EnrichAsync |
| UserContextEnricher(IUserContextProvider userProvider) : class | UserContextEnricher |
| EnrichAsync(ProxyContext context, CancellationToken cancellationToken = default) : async Task | EnrichAsync |

### Ifx.Integrations.Storage.Ftp
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |
| Dispose() : void | Dispose |
| OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task<Stream> | OpenReadAsync |
| WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default) : async Task<StorageObjectMetadata> | WriteAsync |
| GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task<StorageObjectMetadata?> | GetMetadataAsync |
| DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task | DeleteAsync |
| ListAsync(StorageListRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<StorageObjectMetadata> | ListAsync |
| FileExists(string path) : bool | FileExists |
| FileExists(FileInfo fileInfo) : bool | FileExists |
| ReadAllText(string path) : string | ReadAllText |
| ReadAllTextAsync(string path, CancellationToken cancellationToken = default) : async Task<string> | ReadAllTextAsync |
| ReadAllBytes(string path) : byte[] | ReadAllBytes |
| ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) : async Task<byte[]> | ReadAllBytesAsync |
| WriteAllText(string path, string content) : void | WriteAllText |
| WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) : Task | WriteAllTextAsync |
| WriteAllBytes(string path, byte[] bytes) : void | WriteAllBytes |
| WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) : async Task | WriteAllBytesAsync |
| DeleteFile(string path) : void | DeleteFile |
| DeleteFileAsync(string path, CancellationToken cancellationToken = default) : Task | DeleteFileAsync |
| DirectoryExists(string path) : bool | DirectoryExists |
| CreateDirectory(string path) : DirectoryInfo | CreateDirectory |
| CreateDirectoryAsync(string path, CancellationToken cancellationToken = default) : async Task<DirectoryInfo> | CreateDirectoryAsync |
| DeleteDirectory(string path, bool recursive = true) : void | DeleteDirectory |
| DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default) : Task | DeleteDirectoryAsync |
| GetFiles(string path, string searchPattern = "*") : string[] | GetFiles |
| GetDirectories(string path, string searchPattern = "*") : string[] | GetDirectories |
| EnumerateFilesAsync(string path, string searchPattern = "*", [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| GetFullPath(string path) : string | GetFullPath |
| GetDirectoryName(string path) : string? | GetDirectoryName |
| GetFileName(string path) : string | GetFileName |

### Ifx.Integrations.Storage.Ftp.Registration
| Method | Intent |
|--------|--------|
| AddFtpStorage(this IServiceCollection services, FtpStorageOptions options) : static IServiceCollection | AddFtpStorage |
| AddNamedFtpStorage(this IServiceCollection services, string name, FtpStorageOptions options) : static IServiceCollection | AddNamedFtpStorage |
| AddFtp(this StorageRegistrationBuilder builder, string name, FtpStorageOptions options) : static StorageRegistrationBuilder | AddFtp |

### Ifx.Integrations.Transport.Grpc.Dispatch
| Method | Intent |
|--------|--------|
| InvokeAsync(string payload, string requestType) : Task<string> | InvokeAsync |
| InvokeAsync(string payload, string requestType, CancellationToken cancellationToken) : async Task<string> | InvokeAsync |

### Ifx.Integrations.Windows.Messaging.Msmq
| Method | Intent |
|--------|--------|
| Serialize(IMessage message) : static SerializedMsmqMessage | Serialize |
| Deserialize(string body) : static MessageEnvelope | Deserialize |
| HandleAsync(TMessage message, CancellationToken cancellationToken = default) : Task | HandleAsync |
| StartAsync(CancellationToken cancellationToken = default) : Task | StartAsync |
| StopAsync(CancellationToken cancellationToken = default) : async Task | StopAsync |
| Dispose() : void | Dispose |
| StartAsync(CancellationToken cancellationToken) : async Task | StartAsync |
| StopAsync(CancellationToken cancellationToken) : async Task | StopAsync |
| Validate() : void | Validate |
| SendAsync(string queuePath, SerializedMsmqMessage message, CancellationToken cancellationToken) : Task | SendAsync |
| ReceiveAsync(string queuePath, TimeSpan receiveTimeout, CancellationToken cancellationToken) : Task<ReceivedMsmqMessage?> | ReceiveAsync |
| AddMsmqMessaging(
        this IServiceCollection services,
        Action<MsmqOptions>? configure = null) : static IServiceCollection | AddMsmqMessaging |

### Ifx.Integrations.obj.Debug.net10.0-windows.Transport.Grpc.Dispatch.Abstractions
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Integrations.obj.Debug.net10.0.Transport.Grpc.Dispatch
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Integrations.obj.Debug.net10.0.Transport.Grpc.Dispatch.Abstractions
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Integrations.obj.Release.net10.0-windows.Transport.Grpc.Dispatch.Abstractions
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Integrations.obj.Release.net10.0.Transport.Grpc.Dispatch
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Integrations.obj.Release.net10.0.Transport.Grpc.Dispatch.Abstractions
| Method | Intent |
|--------|--------|
| Clone() : InvokeRequest | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeRequest other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeRequest other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| Clone() : InvokeResponse | Clone |
| Equals(object other) : override bool | Equals |
| Equals(InvokeResponse other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| WriteTo(pb::CodedOutputStream output) : void | WriteTo |
| CalculateSize() : int | CalculateSize |
| MergeFrom(InvokeResponse other) : void | MergeFrom |
| MergeFrom(pb::CodedInputStream input) : void | MergeFrom |
| BindService(grpc::ServiceBinderBase serviceBinder, GenericInvokerBase serviceImpl) : static void | BindService |

### Ifx.Roslyn.Analyzers.Helpers
| Method | Intent |
|--------|--------|
| CountStatements(MethodDeclarationSyntax method) : static int | CountStatements |
| CountParameters(MethodDeclarationSyntax method) : static int | CountParameters |
| CountLocalVariables(MethodDeclarationSyntax method) : static int | CountLocalVariables |
| ComputeCyclomaticComplexity(MethodDeclarationSyntax method) : static int | ComputeCyclomaticComplexity |
| ComputeNestingDepth(MethodDeclarationSyntax method) : static int | ComputeNestingDepth |
| TrackAssignment(AssignmentExpressionSyntax assignment) : void | TrackAssignment |
| IsTainted(string identifier) : bool | IsTainted |
| IsSanitized(string identifier) : bool | IsSanitized |
| MarkSanitized(string identifier) : void | MarkSanitized |
| FindSqlInjectionVectorsInExpression(ExpressionSyntax expression) : bool | FindSqlInjectionVectorsInExpression |
| FindLoggingInjectionVectorsInExpression(ExpressionSyntax expression) : bool | FindLoggingInjectionVectorsInExpression |
| IsControllerMethod(MethodDeclarationSyntax method) : static bool | IsControllerMethod |
| IsLoggingOrActivityMethod(InvocationExpressionSyntax invocation) : static bool | IsLoggingOrActivityMethod |
| GetLoggingMethodName(InvocationExpressionSyntax invocation) : static string? | GetLoggingMethodName |
| ExtractIdentifierName(ExpressionSyntax expression) : static string? | ExtractIdentifierName |
| GetProjectType(string projectName) : static ProjectType | GetProjectType |
| GetLayer(string projectName) : static Layer | GetLayer |
| GetVolatilityLevel(ProjectType projectType) : static VolatilityLevel | GetVolatilityLevel |
| IsDependencyAllowed(ProjectType from, ProjectType to) : static bool | IsDependencyAllowed |
| IsLayerDependencyAllowed(Layer from, Layer to) : static bool | IsLayerDependencyAllowed |
| GetProjectTypeName(ProjectType projectType) : static string | GetProjectTypeName |
| IsUnsafeSqlOperation(InvocationExpressionSyntax invocation) : static bool | IsUnsafeSqlOperation |
| IsSafeSqlOperation(InvocationExpressionSyntax invocation) : static bool | IsSafeSqlOperation |
| IsSanitizingMethod(InvocationExpressionSyntax invocation) : static bool | IsSanitizingMethod |
| GetSqlCommandArgument(InvocationExpressionSyntax invocation) : static ExpressionSyntax? | GetSqlCommandArgument |
| GetConcatenationComponents(BinaryExpressionSyntax binary) : static List<ExpressionSyntax> | GetConcatenationComponents |
| GetMethodName(InvocationExpressionSyntax invocation) : static string? | GetMethodName |

### Ifx.Roslyn.Analyzers.Rules
| Method | Intent |
|--------|--------|
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext ctx) : override void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : override void | Initialize |
| Contains(ISymbol? symbol) : static bool | Contains |
| Initialize(AnalysisContext context) : override void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |
| Initialize(AnalysisContext context) : static void | Initialize |

### Ifx.Roslyn.CodeFixes.Common
| Method | Intent |
|--------|--------|
| GetFixAllProvider() : override FixAllProvider? | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override Task | RegisterCodeFixesAsync |

### Ifx.Roslyn.CodeFixes.Providers
| Method | Intent |
|--------|--------|
| GetFixAllProvider() : override FixAllProvider? | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override async Task | RegisterCodeFixesAsync |
| GetFixAllProvider() : override FixAllProvider | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override async Task | RegisterCodeFixesAsync |
| GetSupportedFixAllScopes() : override IEnumerable<FixAllScope> | GetSupportedFixAllScopes |
| GetFixAsync(FixAllContext context) : override async Task<CodeAction?> | GetFixAsync |
| GetFixAllProvider() : override FixAllProvider? | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override async Task | RegisterCodeFixesAsync |
| GetFixAllProvider() : override FixAllProvider? | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override async Task | RegisterCodeFixesAsync |
| GetFixAllProvider() : override FixAllProvider? | GetFixAllProvider |
| RegisterCodeFixesAsync(CodeFixContext context) : override async Task | RegisterCodeFixesAsync |

### Ifx.Roslyn.Generators
| Method | Intent |
|--------|--------|
| Initialize(IncrementalGeneratorInitializationContext context) : void | Initialize |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| Initialize(IncrementalGeneratorInitializationContext context) : void | Initialize |
| AddInterceptors(this IServiceCollection services) : static IServiceCollection | AddInterceptors |
| Initialize(IncrementalGeneratorInitializationContext context) : void | Initialize |

### Ifx.Roslyn.Generators.Endpoints
| Method | Intent |
|--------|--------|
| Initialize(IncrementalGeneratorInitializationContext context) : void | Initialize |

### Ifx.Roslyn.Reporting.Reporting
| Method | Intent |
|--------|--------|
| Create(ReportRequest request, CancellationToken cancellationToken = default) : static FrameworkReport | Create |
| ToJson(FrameworkReport report) : static string | ToJson |
| ToMarkdown(FrameworkReport report) : static string | ToMarkdown |
| ReportRequest(string Revision, string Scope, string[] ExpectedPackages,
    string? CoverageJson, PackageEvidence[] Packages) : sealed record | ReportRequest |
| PackageEvidence(string Package, string? SarifJson, SourceEvidence[]? Sources) : sealed record | PackageEvidence |
| SourceEvidence(string Path, string Text, string[] Symbols) : sealed record | SourceEvidence |
| FrameworkReport(int SchemaVersion, string Revision, string Scope, string MetricPolicy,
    string? CoverageSha256, bool Passed, string[] Issues, PackageReport[] Packages) : sealed record | FrameworkReport |
| PackageReport(string Package, string? SarifSha256, CoverageMeasure? Coverage,
    DiagnosticFact[] Diagnostics, SourceFact[] Sources, MethodMetric[] Methods) : sealed record | PackageReport |
| CoverageMeasure(long LinesCovered, long LinesTotal, long BranchesCovered,
    long BranchesTotal, decimal? LinePercent, decimal? BranchPercent, string Status) : sealed record | CoverageMeasure |
| DiagnosticFact(string RuleId, string Level, string Message, bool Suppressed,
    string? MetricName, string? MetricVersion, string? MeasuredValue, string? Threshold) : sealed record | DiagnosticFact |
| SourceFact(string Path, string Sha256, string? Exclusion) : sealed record | SourceFact |
| MethodMetric(string Path, int StartLine, string Name, int? CyclomaticComplexity,
    int? NestingDepth, int? MethodLength, int ParameterCount, string Status) : sealed record | MethodMetric |

### ifx.Archiving
| Method | Intent |
|--------|--------|
| ArchiveEntry(
    string Name,
    long? Length,
    DateTimeOffset? LastModified) : sealed record | ArchiveEntry |
| ArchiveExtractionResult(IReadOnlyList<string> ExtractedFiles) : sealed record | ArchiveExtractionResult |

### ifx.Archiving.GZip
| Method | Intent |
|--------|--------|
| CanRead(string archivePath) : bool | CanRead |
| ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default) : async Task<ArchiveExtractionResult> | ExtractAsync |
| ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default) : async Task<IReadOnlyList<ArchiveEntry>> | ListEntriesAsync |
| AddGZipArchiveProvider(this IServiceCollection services) : static IServiceCollection | AddGZipArchiveProvider |

### ifx.Archiving.Tar
| Method | Intent |
|--------|--------|
| AddTarArchiveProvider(this IServiceCollection services) : static IServiceCollection | AddTarArchiveProvider |
| CanRead(string archivePath) : bool | CanRead |
| ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default) : async Task<ArchiveExtractionResult> | ExtractAsync |
| ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default) : async Task<IReadOnlyList<ArchiveEntry>> | ListEntriesAsync |

### ifx.Archiving.Zip
| Method | Intent |
|--------|--------|
| AddZipArchiveProvider(this IServiceCollection services) : static IServiceCollection | AddZipArchiveProvider |
| CanRead(string archivePath) : bool | CanRead |
| ExtractAsync(
        string archivePath,
        string destinationDirectory,
        ArchiveExtractionOptions? options = null,
        CancellationToken cancellationToken = default) : async Task<ArchiveExtractionResult> | ExtractAsync |
| ListEntriesAsync(
        string archivePath,
        CancellationToken cancellationToken = default) : Task<IReadOnlyList<ArchiveEntry>> | ListEntriesAsync |

### ifx.Component
| Method | Intent |
|--------|--------|
| Dispose() : void | Dispose |
| ComponentRequest(ILogger<ComponentRequest> logger) : class | ComponentRequest |
| Success() : static ComponentResult | Success |
| Failure(string errorMessage) : static ComponentResult | Failure |
| Failure(Exception exception) : static ComponentResult | Failure |
| Failure(string errorMessage, Exception exception) : static ComponentResult | Failure |
| Match(Action onSuccess, Action<string, Exception?> onFailure) : void | Match |
| ComponentResultBase(bool isSuccess, string? errorMessage, Exception? exception) : abstract class | ComponentResultBase |
| Success(T value) : static ComponentResult<T> | Success |
| Failure(string errorMessage) : static ComponentResult<T> | Failure |
| Failure(Exception exception) : static ComponentResult<T> | Failure |
| Failure(string errorMessage, Exception exception) : static ComponentResult<T> | Failure |
| Match(Action<T> onSuccess, Action<string, Exception?> onFailure) : void | Match |

### ifx.Errors
| Method | Intent |
|--------|--------|
| Error(string Name, Code<Error> Code, string Description, IsRecoverableError IsRecoverableError) : record | Error |
| GetEnumerator() : IEnumerator<Error> | GetEnumerator |
| FromValue(int value) : static IsRecoverableError | FromValue |
| FromName(string? name) : static IsRecoverableError | FromName |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |

### ifx.Extensions
| Method | Intent |
|--------|--------|
| AddConnectionString(this IServiceCollection services, IConfiguration configuration, string connectionName) : static IServiceCollection | AddConnectionString |
| AddNamedConnectionString(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionName,
        string serviceName) : static IServiceCollection | AddNamedConnectionString |
| AddConnectionStringFromSecret(this IServiceCollection services, string secretName) : static IServiceCollection | AddConnectionStringFromSecret |
| GetProceedingWeekday(this DateTime input, DayOfWeek dayOfWeek = DayOfWeek.Sunday) : static DateTime | GetProceedingWeekday |
| GetPreviousWeekday(this DateTime input, DayOfWeek dayOfWeek) : static DateTime | GetPreviousWeekday |
| GetDateOnly(this DateTime dateTime, TimeSpan offset, ShiftDate shiftDate = ShiftDate.ToPast) : static DateTime | GetDateOnly |
| IsWeekend(this DateTime dateTime) : static bool | IsWeekend |
| IsWeekday(this DateTime dateTime) : static bool | IsWeekday |
| GetStartOfWeek(this DateTime dateTime, DayOfWeek startOfWeek = DayOfWeek.Monday) : static DateTime | GetStartOfWeek |
| Next(this Month month) : static Month | Next |
| Previous(this Month month) : static Month | Previous |
| IsInQuarter(this Month month, int quarter) : static bool | IsInQuarter |
| GetQuarter(this Month month) : static int | GetQuarter |
| IsSummerMonth(this Month month) : static bool | IsSummerMonth |
| ToMonth(this DateTime date) : static Month | ToMonth |
| NameOfCallingClass() : static string | NameOfCallingClass |
| TypeOfCallingClass() : static Type? | TypeOfCallingClass |
| ImplementsInterface(this Type type, Type interfaceType) : static bool | ImplementsInterface |
| InvokeMethod(this object obj, string methodName, params object[] parameters) : static object? | InvokeMethod |
| GetUnderlyingType(this Type type) : static Type | GetUnderlyingType |

### ifx.Filtering
| Method | Intent |
|--------|--------|
| Translate(Expression expression) : static FilterNode | Translate |
| Where(Expression<Func<T, bool>> predicate) : FilterBuilder<T> | Where |
| Build() : FilterNode | Build |
| FilterCollectionCondition(string Path, FilterOperation Operator, FilterNode? Predicate) : sealed record | FilterCollectionCondition |
| FilterCondition(string Path, FilterOperation Operator, string? Value) : sealed record | FilterCondition |
| FilterConstant(bool Value) : sealed record | FilterConstant |
| FilterGroup(FilterCombination Combination, IReadOnlyList<FilterNode> Children) : sealed record | FilterGroup |
| Where(Expression<Func<T, bool>> predicate) : FilterGroupBuilder<T> | Where |
| And(Expression<Func<T, bool>> predicate) : FilterGroupBuilder<T> | And |
| Or(Expression<Func<T, bool>> predicate) : FilterGroupBuilder<T> | Or |
| Not(Expression<Func<T, bool>> predicate) : FilterGroupBuilder<T> | Not |
| FilterNegation(FilterNode Operand) : sealed record | FilterNegation |
| Where(Expression<Func<T, bool>> predicate) : FilterSpec<T> | Where |
| And(FilterSpec<T> other) : FilterSpec<T> | And |
| Or(FilterSpec<T> other) : FilterSpec<T> | Or |
| Not() : FilterSpec<T> | Not |
| ToFilterNode() : FilterNode | ToFilterNode |
| FromFilterNode(FilterNode? filter) : static FilterSpec<T> | FromFilterNode |
| Apply(IQueryable<T> source) : IQueryable<T> | Apply |
| Apply(IEnumerable<T> source) : IEnumerable<T> | Apply |

### ifx.Logging
| Method | Intent |
|--------|--------|
| LogCritical(string message, params object[] args) : delegate void | LogCritical |
| LogDebug(string message, params object[] args) : delegate void | LogDebug |
| LogError(string message, params object[] args) : delegate void | LogError |
| LogTraceMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogTraceMessage |
| LogDebugMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogDebugMessage |
| LogInformationMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogInformationMessage |
| LogWarningMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogWarningMessage |
| LogErrorMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogErrorMessage |
| LogCriticalMessage(ILogger logger, string logMessage, Exception? exception = null) : static void | LogCriticalMessage |
| Log(ILogger logger, string logMessage, LogLevel logLevel = LogLevel.Debug, Exception? exception = null) : static void | Log |
| LogTraceMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogTraceMessageAsync |
| LogDebugMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogDebugMessageAsync |
| LogInformationMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogInformationMessageAsync |
| LogWarningMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogWarningMessageAsync |
| LogErrorMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogErrorMessageAsync |
| LogCriticalMessageAsync(ILogger logger, string logMessage, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogCriticalMessageAsync |
| LogAsync(ILogger logger, string logMessage, LogLevel logLevel = LogLevel.Debug, Exception? exception = null, CancellationToken cancellationToken = default) : static async Task | LogAsync |
| LogInformation(string message, params object[] args) : delegate void | LogInformation |
| LogNone(string message, params object[] args) : delegate void | LogNone |
| LogTrace(string message, params object[] args) : delegate void | LogTrace |
| LogWarning(string message, params object[] args) : delegate void | LogWarning |

### ifx.Messaging.Channels
| Method | Intent |
|--------|--------|
| HandleAsync(TMessage message, CancellationToken cancellationToken = default) : Task | HandleAsync |
| StartAsync(CancellationToken cancellationToken = default) : Task | StartAsync |
| StopAsync(CancellationToken cancellationToken = default) : async Task | StopAsync |
| Dispose() : void | Dispose |
| StartAsync(CancellationToken cancellationToken) : async Task | StartAsync |
| StopAsync(CancellationToken cancellationToken) : async Task | StopAsync |
| HandleAsync(TMessage message, CancellationToken cancellationToken) : async Task | HandleAsync |
| AddChannelMessaging(this IServiceCollection services) : static IServiceCollection | AddChannelMessaging |

### ifx.Pagination
| Method | Intent |
|--------|--------|
| PageRequest(int pageNumber = 1, int pageSize = 50, string? continuationToken = null) : sealed class | PageRequest |

### ifx.Pipeline
| Method | Intent |
|--------|--------|
| EndpointResolution(bool IsLocal, string? ServiceName = null, Uri? Uri = null) : record | EndpointResolution |
| Lookup(Type requestType) : ServiceEntry? | Lookup |
| Lookup(Type requestType) : ServiceEntry? | Lookup |
| HttpUri(Uri uri) : static Uri | HttpUri |
| RegistryBasedResolver(IServiceRegistry registry) : sealed class | RegistryBasedResolver |
| Resolve(Type requestType) : EndpointResolution | Resolve |

### ifx.Pipeline.Dispatch
| Method | Intent |
|--------|--------|
| HttpRemoteDispatcher(HttpClient http, ISerializer serializer) : sealed class | HttpRemoteDispatcher |
| LocalDispatcher(IServiceProvider sp) : sealed class | LocalDispatcher |

### ifx.Pipeline.Observibility
| Method | Intent |
|--------|--------|
| IncrementCounter(string metric, string label) : void | IncrementCounter |
| ObserveHistogram(string metric, string label, long valueMs) : void | ObserveHistogram |
| StartSpan(string name) : ISpan | StartSpan |
| SetTag(string key, string value) : void | SetTag |
| End() : void | End |
| Dispose() : void | Dispose |

### ifx.Primitives
| Method | Intent |
|--------|--------|
| Create(string? value) : static Code<TOwner> | Create |
| Parse(string? text) : static Code<TOwner> | Parse |
| TryParse(string? text, out Code<TOwner> value) : static bool | TryParse |
| string(Code<TOwner> code) : static explicit operator | string |
| ToString() : override string | ToString |
| Equals(Code<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| Equals(object? obj) : override bool | Equals |
| Equals(ConnectionString? other) : bool | Equals |
| GetHashCode() : override int | GetHashCode |
| string(ConnectionString connectionString) : static implicit operator | string |
| ConnectionString(string connectionString) : static explicit operator | ConnectionString |
| ToString() : override string | ToString |
| TKey(EntityIdentifier<TEntity, TKey> identifier) : static explicit operator | TKey |
| TryParse(string text, out EntityIdentifier<TEntity, TKey> identifier) : static bool | TryParse |
| CanConvert(Type typeToConvert) : override bool | CanConvert |
| CreateConverter(Type type, JsonSerializerOptions options) : override JsonConverter | CreateConverter |
| Write(Utf8JsonWriter writer, EntityIdentifier<TEntity, TKey> value, JsonSerializerOptions options) : override void | Write |
| Enumeration(int id, string name) : abstract class | Enumeration |
| ToString() : override string | ToString |
| Create(Guid value) : static Identifier<TOwner> | Create |
| New() : static Identifier<TOwner> | New |
| NewSequential() : static Identifier<TOwner> | NewSequential |
| Parse(string? text) : static Identifier<TOwner> | Parse |
| TryParse(string? text, out Identifier<TOwner> identifier) : static bool | TryParse |
| Guid(Identifier<TOwner> identifier) : static explicit operator | Guid |
| ToString() : override string | ToString |
| Equals(Identifier<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| CanConvert(Type typeToConvert) : override bool | CanConvert |
| CreateConverter(Type type, JsonSerializerOptions options) : override JsonConverter | CreateConverter |
| Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) : override Identifier<TOwner> | Read |
| Write(Utf8JsonWriter writer, Identifier<TOwner> value, JsonSerializerOptions options) : override void | Write |
| Create(decimal amount, string currency) : static Money | Create |
| Zero(string currency) : static Money | Zero |
| Parse(string? text) : static Money | Parse |
| TryParse(string? text, out Money? money) : static bool | TryParse |
| Add(Money other) : Money | Add |
| Subtract(Money other) : Money | Subtract |
| decimal(Money money) : static explicit operator | decimal |
| ToString() : override string | ToString |
| Equals(Money? other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| ToString() : override string | ToString |
| Create(string? value) : static NonEmptyText<TOwner> | Create |
| Parse(string? text) : static NonEmptyText<TOwner> | Parse |
| TryParse(string? text, out NonEmptyText<TOwner> value) : static bool | TryParse |
| string(NonEmptyText<TOwner> text) : static explicit operator | string |
| ToString() : override string | ToString |
| Equals(NonEmptyText<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| Create(decimal value) : static NonNegativeAmount<TOwner> | Create |
| Parse(string? text) : static NonNegativeAmount<TOwner> | Parse |
| TryParse(string? text, out NonNegativeAmount<TOwner> value) : static bool | TryParse |
| decimal(NonNegativeAmount<TOwner> amount) : static explicit operator | decimal |
| ToString() : override string | ToString |
| Equals(NonNegativeAmount<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| Create(decimal value) : static Percentage<TOwner> | Create |
| Parse(string? text) : static Percentage<TOwner> | Parse |
| TryParse(string? text, out Percentage<TOwner> value) : static bool | TryParse |
| decimal(Percentage<TOwner> percentage) : static explicit operator | decimal |
| ToString() : override string | ToString |
| Equals(Percentage<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |
| Create(int value) : static PositiveQuantity<TOwner> | Create |
| Parse(string? text) : static PositiveQuantity<TOwner> | Parse |
| TryParse(string? text, out PositiveQuantity<TOwner> value) : static bool | TryParse |
| int(PositiveQuantity<TOwner> quantity) : static explicit operator | int |
| ToString() : override string | ToString |
| Equals(PositiveQuantity<TOwner> other) : bool | Equals |
| Equals(object? obj) : override bool | Equals |
| GetHashCode() : override int | GetHashCode |

### ifx.Primitives.Workday
| Method | Intent |
|--------|--------|
| ToString() : override string | ToString |
| string(Currency value) : static explicit operator | string |
| Currency(string value) : static explicit operator | Currency |
| Contains(DateOnly date) : bool | Contains |
| Overlaps(DateRange other) : bool | Overlaps |
| ToString() : override string | ToString |
| Deconstruct(out DateTime start, out DateTime end) : void | Deconstruct |
| Contains(DateTime value) : bool | Contains |
| IsPast(DateTime value) : bool | IsPast |
| IsFuture(DateTime value) : bool | IsFuture |
| IsCurrent(DateTime value) : bool | IsCurrent |
| Overlaps(DateTimeRange other) : bool | Overlaps |
| TryCreate(string? value, [NotNullWhen(true) : static bool | TryCreate |
| ToUri() : Uri | ToUri |
| ToString() : override string | ToString |
| string(HttpUrl value) : static explicit operator | string |
| HttpUrl(string value) : static explicit operator | HttpUrl |
| New() : static Identity<T> | New |
| Parse(string value) : static Identity<T> | Parse |
| TryParse(string? value, [NotNullWhen(true) : static bool | TryParse |
| ToString() : override string | ToString |
| Guid(Identity<T> value) : static explicit operator | Guid |
| Zero(Currency currency) : static Money | Zero |
| Round(int decimalPlaces, MidpointRounding rounding) : Money | Round |
| Apply(Percentage percentage) : Money | Apply |
| ToString() : override string | ToString |
| TryCreate(string? value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| string(NonEmptyString value) : static explicit operator | string |
| NonEmptyString(string value) : static explicit operator | NonEmptyString |
| IsValid(T value) : static bool | IsValid |
| TryCreate(T value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| T(NonNegativeValue<T> value) : static explicit operator | T |
| IsValid(int value) : static bool | IsValid |
| TryCreate(int value, [NotNullWhen(true) : static bool | TryCreate |
| GetOffset(PageSize size) : long | GetOffset |
| ToString() : override string | ToString |
| int(PageNumber value) : static explicit operator | int |
| PageNumber(int value) : static explicit operator | PageNumber |
| IsValid(int value) : static bool | IsValid |
| TryCreate(int value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| int(PageSize value) : static explicit operator | int |
| PageSize(int value) : static explicit operator | PageSize |
| IsValid(decimal value) : static bool | IsValid |
| TryCreate(decimal value, [NotNullWhen(true) : static bool | TryCreate |
| ToRatio() : Ratio | ToRatio |
| ApplyTo(decimal value) : decimal | ApplyTo |
| ToString() : override string | ToString |
| decimal(Percentage value) : static explicit operator | decimal |
| Percentage(decimal value) : static explicit operator | Percentage |
| IsValid(int value) : static bool | IsValid |
| TryCreate(int value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| int(PortNumber value) : static explicit operator | int |
| PortNumber(int value) : static explicit operator | PortNumber |
| IsValid(T value) : static bool | IsValid |
| TryCreate(T value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| T(PositiveValue<T> value) : static explicit operator | T |
| IsValid(decimal value) : static bool | IsValid |
| TryCreate(decimal value, [NotNullWhen(true) : static bool | TryCreate |
| ToPercentage() : Percentage | ToPercentage |
| ApplyTo(decimal value) : decimal | ApplyTo |
| ToString() : override string | ToString |
| decimal(Ratio value) : static explicit operator | decimal |
| Ratio(decimal value) : static explicit operator | Ratio |
| IsValid(string? value) : static bool | IsValid |
| TryCreate(string? value, [NotNullWhen(true) : static bool | TryCreate |
| ToString() : override string | ToString |
| string(ResponseCode value) : static explicit operator | string |
| ResponseCode(string value) : static explicit operator | ResponseCode |
| Reveal() : string | Reveal |
| ToString() : override string | ToString |
| Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) : override Secret | Read |
| Write(Utf8JsonWriter writer, Secret value, JsonSerializerOptions options) : override void | Write |
| Now(TimeProvider? clock = null) : static UtcTimestamp | Now |
| Add(TimeSpan duration) : UtcTimestamp | Add |
| ToString() : override string | ToString |
| DateTimeOffset(UtcTimestamp value) : static explicit operator | DateTimeOffset |
| UtcTimestamp(DateTimeOffset value) : static explicit operator | UtcTimestamp |

### ifx.Providers
| Method | Intent |
|--------|--------|
| GenerateNew() : string | GenerateNew |
| SetCorrelationId(string correlationId) : void | SetCorrelationId |
| GenerateNew() : string | GenerateNew |
| SetRequestId(string requestId) : void | SetRequestId |

### ifx.Proxy
| Method | Intent |
|--------|--------|
| DefaultProxyPipeline(IEnumerable<IProxyInterceptor> interceptors, IProxyTransport transport) : sealed class | DefaultProxyPipeline |
| AddProxyPipeline() : IServiceCollection | AddProxyPipeline |
| Set(ProxyContext context, string name, string value) : static void | Set |
| Success(T data) : static ProxyResponse<T> | Success |
| Success(T data, int statusCode) : static ProxyResponse<T> | Success |
| Failure(string errorMessage) : static ProxyResponse<T> | Failure |

### ifx.Proxy.Attributes
| Method | Intent |
|--------|--------|
| ProxyInterceptorOrderAttribute(int order) : sealed class | ProxyInterceptorOrderAttribute |

### ifx.Proxy.Http.Transports
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |

### ifx.Querying
| Method | Intent |
|--------|--------|
| Rebind(Expression body, ParameterExpression source, ParameterExpression target) : static Expression | Rebind |
| User(int Id, string Name, string Email) : sealed record | User |
| Validate(string json) : static IReadOnlyList<string> | Validate |
| IsValid(JsonDocument jsonDocument) : static bool | IsValid |
| For(IQueryable<T> source) : static QueryPlan<T> | For |
| Where(Expression<Func<T, bool>> predicate) : QueryPlan<T> | Where |
| Where(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QueryPlan<T> | Where |
| And(Expression<Func<T, bool>> predicate) : QueryPlan<T> | And |
| And(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QueryPlan<T> | And |
| Or(Expression<Func<T, bool>> predicate) : QueryPlan<T> | Or |
| Or(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QueryPlan<T> | Or |
| Not(Expression<Func<T, bool>> predicate) : QueryPlan<T> | Not |
| Not(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QueryPlan<T> | Not |
| ApplyFilter(FilterNode? node) : QueryPlan<T> | ApplyFilter |
| ApplySort(IEnumerable<SortClause>? clauses) : QueryPlan<T> | ApplySort |
| AfterCursor(string? cursor) : QueryPlan<T> | AfterCursor |
| BuildCursor(T lastItem) : string | BuildCursor |
| ToCursorPage(int pageSize) : CursorPage<T> | ToCursorPage |
| SkipItems(int count) : QueryPlan<T> | SkipItems |
| TakeItems(int count) : QueryPlan<T> | TakeItems |
| Page(int pageNumber, int pageSize) : QueryPlan<T> | Page |
| ToQueryable() : IQueryable<T> | ToQueryable |
| ToUnpagedQueryable() : IQueryable<T> | ToUnpagedQueryable |
| For(IQueryable<T> source) : static QuerySpec<T> | For |
| Where(Expression<Func<T, bool>> predicate) : QuerySpec<T> | Where |
| Where(FilterSpec<T> filter) : QuerySpec<T> | Where |
| Where(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QuerySpec<T> | Where |
| Or(Expression<Func<T, bool>> predicate) : QuerySpec<T> | Or |
| Or(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QuerySpec<T> | Or |
| Not(Expression<Func<T, bool>> predicate) : QuerySpec<T> | Not |
| Not(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QuerySpec<T> | Not |
| And(Expression<Func<T, bool>> predicate) : QuerySpec<T> | And |
| And(Func<FilterGroupBuilder<T>, FilterGroupBuilder<T>> group) : QuerySpec<T> | And |
| ApplyFilter(FilterNode? filter) : QuerySpec<T> | ApplyFilter |
| Page(int offset, int size) : QuerySpec<T> | Page |
| SkipItems(int count) : QuerySpec<T> | SkipItems |
| TakeItems(int count) : QuerySpec<T> | TakeItems |
| AfterCursor(string? cursor) : QuerySpec<T> | AfterCursor |
| ApplySort(IEnumerable<SortClause>? clauses) : QuerySpec<T> | ApplySort |
| BuildCursor(T lastItem) : string | BuildCursor |
| ToCursorPage(int pageSize) : CursorPage<T> | ToCursorPage |
| ToQueryable() : IQueryable<T> | ToQueryable |
| ToUnpagedQueryable() : IQueryable<T> | ToUnpagedQueryable |
| Apply(IQueryable<T> source) : IQueryable<T> | Apply |
| Apply(IQueryable<T> query) : IQueryable<TResult> | Apply |
| SortClause(string Path, SortDirection Direction = SortDirection.Ascending) : sealed record | SortClause |

### ifx.Querying.Serialization
| Method | Intent |
|--------|--------|
| CompositeFilter(string Operator, List<FilterNode> Children) : sealed record | CompositeFilter |
| PropertyFilter(string Operator, string Property, string? Value, bool IgnoreCase = false) : sealed record | PropertyFilter |
| SaveToFile(string path) : static void | SaveToFile |
| Serialize(FilterNode node) : static string | Serialize |
| Deserialize(string json) : static FilterNode? | Deserialize |
| Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) : override FilterNode | Read |
| Write(Utf8JsonWriter writer, FilterNode value, JsonSerializerOptions options) : override void | Write |

### ifx.Secrets
| Method | Intent |
|--------|--------|
| GetAsync(string name, CancellationToken cancellationToken = default) : Task<string?> | GetAsync |

### ifx.Secrets.Local
| Method | Intent |
|--------|--------|
| GetAsync(string name, CancellationToken cancellationToken = default) : Task<string?> | GetAsync |

### ifx.Storage
| Method | Intent |
|--------|--------|
| StorageComponent(ILogger<StorageComponent> logger) : class | StorageComponent |
| FileExists(string path) : bool | FileExists |
| FileExists(FileInfo fileInfo) : bool | FileExists |
| ReadAllText(string path) : string | ReadAllText |
| ReadAllTextAsync(string path) : async Task<string> | ReadAllTextAsync |
| ReadAllTextAsync(string path, CancellationToken cancellationToken) : async Task<string> | ReadAllTextAsync |
| ReadAllBytes(string path) : byte[] | ReadAllBytes |
| ReadAllBytesAsync(string path) : async Task<byte[]> | ReadAllBytesAsync |
| WriteAllText(string path, string content) : void | WriteAllText |
| WriteAllTextAsync(string path, string content) : async Task | WriteAllTextAsync |
| WriteAllBytes(string path, byte[] bytes) : void | WriteAllBytes |
| WriteAllBytesAsync(string path, byte[] bytes) : async Task | WriteAllBytesAsync |
| DeleteFile(string path) : void | DeleteFile |
| DeleteFileAsync(string path) : Task | DeleteFileAsync |
| DirectoryExists(string path) : bool | DirectoryExists |
| CreateDirectory(string path) : DirectoryInfo | CreateDirectory |
| CreateDirectoryAsync(string path) : Task<DirectoryInfo> | CreateDirectoryAsync |
| DeleteDirectory(string path, bool recursive = false) : void | DeleteDirectory |
| DeleteDirectoryAsync(string path, bool recursive = false) : Task | DeleteDirectoryAsync |
| GetFiles(string path) : string[] | GetFiles |
| GetFiles(string path, string searchPattern) : string[] | GetFiles |
| GetDirectories(string path) : string[] | GetDirectories |
| GetDirectories(string path, string searchPattern) : string[] | GetDirectories |
| EnumerateFilesAsync(string path) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| EnumerateFilesAsync(string path, string searchPattern) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| EnumerateFilesAsync(string path, string searchPattern, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| GetFullPath(string path) : string | GetFullPath |
| GetDirectoryName(string path) : string? | GetDirectoryName |
| GetFileName(string path) : string? | GetFileName |
| AddLocalStorage(this IServiceCollection services, LocalStorageOptions options) : static IServiceCollection | AddLocalStorage |
| AddNamedLocalStorage(this IServiceCollection services, string name, LocalStorageOptions options) : static IServiceCollection | AddNamedLocalStorage |
| Validate() : void | Validate |
| RegisterImplementation(string name, Type implementationType, object? options = null) : void | RegisterImplementation |
| StorageImplementation(Type ImplementationType, object? Options = null) : sealed record | StorageImplementation |
| StorageRegistrationBuilder(IServiceCollection services) : sealed class | StorageRegistrationBuilder |
| AddLocal(string name = "local") : StorageRegistrationBuilder | AddLocal |

### ifx.Storage.Local
| Method | Intent |
|--------|--------|
| Validate() : void | Validate |
| OpenReadAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task<Stream> | OpenReadAsync |
| WriteAsync(StorageWriteRequest request, CancellationToken cancellationToken = default) : async Task<StorageObjectMetadata> | WriteAsync |
| GetMetadataAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task<StorageObjectMetadata?> | GetMetadataAsync |
| DeleteAsync(StorageObjectRequest request, CancellationToken cancellationToken = default) : Task | DeleteAsync |
| ListAsync(StorageListRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<StorageObjectMetadata> | ListAsync |
| FileExists(FileInfo fileInfo) : bool | FileExists |
| FileExists(string path) : bool | FileExists |
| ReadAllText(string path) : string | ReadAllText |
| ReadAllTextAsync(string path, CancellationToken cancellationToken = default) : Task<string> | ReadAllTextAsync |
| ReadAllBytes(string path) : byte[] | ReadAllBytes |
| ReadAllBytesAsync(string path, CancellationToken cancellationToken = default) : Task<byte[]> | ReadAllBytesAsync |
| WriteAllText(string path, string content) : void | WriteAllText |
| WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default) : Task | WriteAllTextAsync |
| WriteAllBytes(string path, byte[] bytes) : void | WriteAllBytes |
| WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default) : Task | WriteAllBytesAsync |
| DeleteFile(string path) : void | DeleteFile |
| DeleteFileAsync(string path, CancellationToken cancellationToken = default) : Task | DeleteFileAsync |
| DirectoryExists(string path) : bool | DirectoryExists |
| CreateDirectory(string path) : DirectoryInfo | CreateDirectory |
| CreateDirectoryAsync(string path, CancellationToken cancellationToken = default) : Task<DirectoryInfo> | CreateDirectoryAsync |
| DeleteDirectory(string path, bool recursive = true) : void | DeleteDirectory |
| DeleteDirectoryAsync(string path, bool recursive = true, CancellationToken cancellationToken = default) : Task | DeleteDirectoryAsync |
| GetFiles(string path, string searchPattern = "*") : string[] | GetFiles |
| GetDirectories(string path, string searchPattern = "*") : string[] | GetDirectories |
| EnumerateFilesAsync(string path, string searchPattern = "*",
        [EnumeratorCancellation] CancellationToken cancellationToken = default) : async IAsyncEnumerable<string> | EnumerateFilesAsync |
| GetFullPath(string path) : string | GetFullPath |
| GetDirectoryName(string path) : string? | GetDirectoryName |
| GetFileName(string path) : string | GetFileName |
