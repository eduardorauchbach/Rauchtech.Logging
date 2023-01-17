using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Rauchtech.Logging
{
    public interface ICustomLog<T> : ICustomLogFactory where T : class
    {
        /// <summary>
        /// Adds Scope Keys that will show in every log, from start to end of this scope (prevents and ignores duplication)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">strings, guids or number</param>
        void AddKey(string key, object value);

        /// <summary>
        /// Add a new log entry for the scope
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message">Any text, does not accept column based logging</param>
        /// <param name="memberName"></param>
        /// <param name="sourceLineNumber"></param>
        /// <param name="args">Any object that converted to json and combined with message stays under the string size limit (36k...)</param>
        public void LogCustom(LogLevel logLevel,
                                LogType logType = LogType.Log,
                                EventId? eventId = null,
                                Exception? exception = null,
                                string? message = null,
                                string? sourceContext = null,
                                [CallerMemberName] string? memberName = null,
                                [CallerLineNumber] int? sourceLineNumber = null,
                                params ValueTuple<string, object>[] args);
    }

    public interface ICustomLogFactory
    {
        /// <summary>
        /// Only needed in case EnableScopeKeys is enabled, and forces all the logs stored to be written
        /// </summary>
        void Finish();
    }
}
