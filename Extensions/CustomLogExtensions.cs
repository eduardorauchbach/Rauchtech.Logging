using Microsoft.Extensions.Logging;
using RauchTech.Logging.Services;
using System.Runtime.CompilerServices;
namespace RauchTech.Logging
{
    public static class CustomLogExtensions
    {
        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogCritical<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Critical,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogDebug<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Debug,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogError<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Error,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogInformation<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Information,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogTrace<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Trace,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public static void LogWarning<T>(this ICustomLog<T> log,
                                        CustomLogType logType = CustomLogType.Log,
                                        EventId? eventId = null,
                                        Exception? exception = null,
                                        string? message = null,
                                        [CallerMemberName] string? memberName = null,
                                        [CallerLineNumber] int? sourceLineNumber = null,
                                        params ValueTuple<string, object>[] args) where T : class
        {
            log.Log(logLevel: LogLevel.Warning,
                logType: logType,
                eventId: eventId,
                exception: exception,
                message: message,
                sourceContext: Helper.NameOfCallingClass(true),
                memberName: memberName,
                sourceLineNumber: sourceLineNumber,
                args: args
                );
        }
    }
}
