// Copyright (c) 2025 VisionaryCoder. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

namespace VisionaryCoder.Framework;

/// <summary>
/// Provides constant values used throughout the VisionaryCoder Framework.
/// </summary>
public static class Constants
{
    
    /// <summary>
    /// The version of the VisionaryCoder Framework.
    /// </summary>
    public const string Version = "1.0.0";

    /// <summary>
    /// The configuration section name for framework settings.
    /// </summary>
    public const string ConfigurationSection = "VisionaryCoderFramework";

    /// <summary>
    /// Timeout-related constants.
    /// </summary>
    public static class Timeouts
    {
        /// <summary>
        /// Default HTTP request timeout in seconds.
        /// </summary>
        public const int DefaultHttpTimeoutSeconds = 30;

        /// <summary>
        /// Default database operation timeout in seconds.
        /// </summary>
        public const int DefaultDatabaseTimeoutSeconds = 30;

        /// <summary>
        /// Default cache expiration time in minutes.
        /// </summary>
        public const int DefaultCacheExpirationMinutes = 15;
    }

    /// <summary>
    /// HTTP header name constants.
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// Correlation ID header for distributed tracing.
        /// </summary>
        public const string CorrelationId = "X-Correlation-ID";

        /// <summary>
        /// Request ID header for request tracking.
        /// </summary>
        public const string RequestId = "X-Request-ID";

        /// <summary>
        /// User context header for user information.
        /// </summary>
        public const string UserContext = "X-User-Context";

        /// <summary>
        /// API version header for versioning support.
        /// </summary>
        public const string ApiVersion = "Api-Version";
    }

    /// <summary>
    /// Logging-related constants.
    /// </summary>
    public static class Logging
    {
        /// <summary>
        /// Default log level for the framework.
        /// </summary>
        public const string DefaultLogLevel = "Information";

        /// <summary>
        /// Log category for framework operations.
        /// </summary>
        public const string FrameworkCategory = "VisionaryCoder.Framework";

        /// <summary>
        /// Log category for performance tracking.
        /// </summary>
        public const string PerformanceCategory = "VisionaryCoder.Framework.Performance";

        /// <summary>
        /// Default structured logging template.
        /// </summary>
        public const string DefaultTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        /// <summary>
        /// Correlation ID property name for logging.
        /// </summary>
        public const string CorrelationIdProperty = "CorrelationId";

        /// <summary>
        /// Request ID property name for logging.
        /// </summary>
        public const string RequestIdProperty = "RequestId";

        /// <summary>
        /// User ID property name for logging.
        /// </summary>
        public const string UserIdProperty = "UserId";
    }
}
