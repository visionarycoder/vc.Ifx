namespace Ifx;

public static class Constants
{
    public const string Version = "1.0.0";
    public const string ConfigurationSection = "vc.IfxFramework";

    public static class Timeouts
    {
        public const int DefaultHttpTimeoutSeconds = 30;
        public const int DefaultDatabaseTimeoutSeconds = 30;
        public const int DefaultCacheExpirationMinutes = 15;
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string RequestId = "X-Request-ID";
        public const string UserContext = "X-User-Context";
        public const string ApiVersion = "Api-Version";
    }

    public static class Logging
    {
        public const string DefaultLogLevel = "Information";
        public const string FrameworkCategory = "Ifx";
        public const string PerformanceCategory = "Ifx.Performance";
        public const string DefaultTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        public const string CorrelationIdProperty = "CorrelationId";
        public const string RequestIdProperty = "RequestId";
        public const string UserIdProperty = "UserId";
    }
}
