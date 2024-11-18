using Eyefinity.Utilities.AuditLogging.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Eyefinity.Utilities.AuditLogging.Helpers
{
    public static partial class Log
    {
        [LoggerMessage(
            EventId = 1200,
            Level = LogLevel.Information,
            Message = "InstanceId = {instanceId}, Message = {message}, Context = {context}, Data = {data}")]
        public static partial void Message(
           this ILogger logger,
           Guid instanceId,
           string message,
           [CallerMemberName] string context = "",
           object? data = null);

        [LoggerMessage(
            EventId = 1201,
            Level = LogLevel.Information,
            Message = "InstanceId = {instanceId}, EventId = {eventId}, Data = {data}, Context = {context}")]
        public static partial void LogAudit(
           this ILogger logger,
           Guid instanceId,
           Guid eventId,
           EntityAudit data,
           [CallerMemberName] string context = "");

        [LoggerMessage(
            EventId = 1202,
            Level = LogLevel.Information,
            Message = "InstanceId = {instanceId}, EventId = {eventId}, Data = {data}, Context = {context}")]
        public static partial void LogFieldAudit(
           this ILogger logger,
           Guid instanceId,
           Guid eventId,
           EntityFieldAudit data,
           [CallerMemberName] string context = "");

        [LoggerMessage(
            EventId = 1300,
            Level = LogLevel.Warning,
            Message = "InstanceId = {instanceId}, Message = {message}, Context = {context}")]
        public static partial void NullArgument(
            this ILogger logger,
            Guid instanceId,
            string message,
            [CallerMemberName] string context = "");

        [LoggerMessage(
            EventId = 1400,
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
