using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Eyefinity.Utilities.FeatureFlagging.Helpers
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 404,
            Level = LogLevel.Error,
            Message = "InstanceId = {instanceId}, Message = {message}, Context = {context}")]
        public static partial void NotFoundExceptionOccurred(
            this ILogger logger,
            Exception ex,
            Guid instanceId,
            string message,
            [CallerMemberName] string context = "");

        [LoggerMessage(
            EventId = 500,
            Level = LogLevel.Error,
            Message = "InstanceId = {instanceId}, Message = {message}, Context = {context}")]
        public static partial void ExceptionOccurred(
            this ILogger logger,
            Exception ex,
            Guid instanceId,
            string message,
            [CallerMemberName] string context = "");
    }
}