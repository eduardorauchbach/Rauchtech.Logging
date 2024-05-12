using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace RauchTech.Logging.Services
{
    public class CustomLog<T> : CustomLog, ICustomLog<T> where T : class
    {
        public CustomLog(ICustomLogFactory customLogFactory, ILoggerFactory loggerFactory, IConfiguration configuration) : base(loggerFactory, configuration)
        {
            ILoggerFactory = ((CustomLogFactory)customLogFactory).ILoggerFactory;
            IDs = ((CustomLogFactory)customLogFactory).IDs;
            LogHistory = ((CustomLogFactory)customLogFactory).LogHistory;

            ILogger = loggerFactory.CreateLogger<T>();
        }
    }

    public class CustomLog : CustomLogFactory
    {
        public CustomLog(ILoggerFactory loggerFactory, IConfiguration configuration) : base(loggerFactory, configuration)
        {
        }

        internal void RegisterContextParameters(ActionExecutingContext context, string sourceContext, string actionName)
        {
            if (MinimumLogLevel <= LogLevel.Debug)
            {
                var args = context.ActionArguments.SelectMany(arg => Helper.RemoveBannedParameters(arg.Key, arg.Value, BannedParameters)).ToArray();
                foreach (var arg in args.SelectMany(arg => Helper.GetIdProperties(arg.Item1, arg.Value)).Distinct())
                {
                    AddKey($"param {arg.Key}", arg.Value);
                }

                if (args.Any())
                {
                    Log(LogLevel.Debug, sourceContext: sourceContext, memberName: actionName, message: CustomLogDefaultMessages.Parameters, args: args);
                }
            }
        }

        public void Log(LogLevel logLevel,
                        CustomLogType logType = CustomLogType.Log,
                        EventId? eventId = null,
                        Exception? exception = null,
                        string? message = null,
                        string? sourceContext = null,
                        [CallerMemberName] string? memberName = null,
                        [CallerLineNumber] int? sourceLineNumber = null,
                        params ValueTuple<string, object>[] args)
        {
            if (ILogger is null)
            {
                throw new NullReferenceException(nameof(ILogger));
            }

            if (MinimumLogLevel <= logLevel)
            {
                ValidateExceptionHistory(exception);

                sourceContext ??= Helper.NameOfCallingClass();

                AddLogHistory(logger: ILogger,
                            logLevel: logLevel,
                            logType: logType,
                            eventId: eventId,
                            exception: exception,
                            message: message,
                            sourceContext: sourceContext,
                            memberName: memberName,
                            sourceLineNumber: sourceLineNumber,
                            args: args
                            );

                if (!EnableScopeKeys)
                {
                    if (LogHistory.LogItems.Count > 1)
                    {
                        LogHistory.LogItems.RemoveAt(0);
                    }

                    InjectLog(LogHistory.LogItems.First());
                }
            }
        }

        private bool ValidateExceptionHistory(Exception exception)
        {
            if (exception != null)
            {
                int exceptionHash = exception.GetHashCode();
                if (LoggedExceptionHistory.Any(x => x.Code == exceptionHash || x.Message == exception.Message))
                {
                    // Exception has already been logged, return without logging again
                    return false;
                }

                // Mark this exception as logged
                LoggedExceptionHistory.Add((exceptionHash, exception.Message));
            }

            return true;
        }
    }

    public class CustomLogFactory : ICustomLogFactory
    {
        #region Log Memory

        public class LogItem
        {
            public ILogger Logger { get; }
            public long CurrentStep { get; }
            public DateTime CurrentTime { get; }

            public LogLevel LogLevel { get; }
            public EventId? EventId { get; }
            public Exception? Exception { get; }
            public CustomLogType LogType { get; }
            public string? Message { get; set; }
            public string? SourceContext { get; set; }
            public string? MemberName { get; }
            public int? SourceLineNumber { get; }

            public ValueTuple<string, object>[] Args { get; }

            public LogItem(ILogger logger,
                            long currentStep,
                            LogLevel logLevel,
                            CustomLogType logType = CustomLogType.Log,
                            EventId? eventId = null,
                            Exception? exception = null,
                            string? message = null,
                            string? sourceContext = null,
                            string? memberName = null,
                            int? sourceLineNumber = null,
                            params ValueTuple<string, object>[] args)
            {
                Logger = logger;
                CurrentStep = currentStep;
                CurrentTime = DateTime.UtcNow;
                LogLevel = logLevel;
                LogType = logType;
                EventId = eventId;
                Exception = exception;
                Message = message;
                SourceContext = sourceContext;
                MemberName = memberName;
                SourceLineNumber = sourceLineNumber;
                Args = args;
            }
        }
        public class CustomLogHistory
        {
            public List<LogItem> LogItems { get; set; } = new List<LogItem>();
        }
        public class CustomLogVault
        {
            public List<(string, object)> Keys { get; set; } = new List<(string, object)>();
        }

        #endregion

        #region Properties

        public static LogLevel MinimumLogLevel { get; set; }
        public static string ApplicationName { get; set; } = string.Empty;
        public static bool EnableScopeKeys { get; set; } = false;
        public static string[] BannedParameters { get; set; } = [];

        public CustomLogVault IDs { get; set; }
        public CustomLogHistory LogHistory { get; set; }

        public ILoggerFactory ILoggerFactory { get; set; }
        public ILogger? ILogger { get; set; }
        #endregion

        protected readonly List<(int Code, string Message)> LoggedExceptionHistory = new();

        #region Constructor

        public CustomLogFactory(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            var logLevelName = configuration["LogLevel"];
            Enum.TryParse(typeof(LogLevel), logLevelName, true, out var tLogLevel);
            MinimumLogLevel = (LogLevel?)tLogLevel ?? LogLevel.Information;

            ApplicationName = configuration["ApplicationName"] ?? "unknown";
            EnableScopeKeys = Convert.ToBoolean(configuration["EnableScopeKeys"] ?? "false");
            BannedParameters = (configuration["LogFiltersBannedParameters"] ?? "").Split(';');

            ILoggerFactory = loggerFactory;
            IDs = new CustomLogVault();
            LogHistory = new CustomLogHistory();
        }

        #endregion

        public void AddKey(string key, object value)
        {
            var id = key.ToSnakeCase();

            if (!IDs.Keys.Any(x => x.Item1 == id))
            {
                IDs.Keys.Add((id, value));
            }
        }

        protected void AddLogHistory(ILogger logger,
                    LogLevel logLevel,
                    CustomLogType logType,
                    EventId? eventId = null,
                    Exception? exception = null,
                    string? message = null,
                    string? sourceContext = null,
                    string? memberName = null,
                    int? sourceLineNumber = 0,
                    params ValueTuple<string, object>[] args)
        {
            long currentStep = LogHistory.LogItems.Count == 0 ? 1 : LogHistory.LogItems.Max(x => x.CurrentStep) + 1;
            LogHistory.LogItems.Add(new LogItem
                                    (
                                        logger: logger,
                                        currentStep: currentStep,
                                        logLevel: logLevel,
                                        logType: logType,
                                        eventId: eventId,
                                        exception: exception,
                                        message: message,
                                        sourceContext: sourceContext,
                                        memberName: memberName,
                                        sourceLineNumber: sourceLineNumber,
                                        args: args
                                    ));
        }

        protected class CustomLogData
        {
            public CustomLogData(string? message = null,
                                 string? method = null,
                                 int? line = null,
                                 params ValueTuple<string, object>[] values)
            {
                Method = method;
                Line = line;
                Message = message;

                if (values.Length > 0)
                {
                    Values = GetDynamicObject(values.ToDictionary(x => x.Item1, x => x.Item2));
                }
            }

            public string? Method { get; }
            public int? Line { get; }
            public string? Message { get; }

            public dynamic? Values { get; }
        }

        protected void InjectLog(LogItem logItem)
        {
            if (string.IsNullOrEmpty(logItem.SourceContext))
            {
                logItem.SourceContext = logItem.Logger.GetType().GetGenericArguments().FirstOrDefault()?.FullName;
            }

            logItem.Message ??= CustomLogDefaultMessages.LineMarker;

            dynamic customLogKeys = GetDynamicObject(IDs.Keys.ToDictionary(x => x.Item1, x => x.Item2));
            CustomLogData customLogData = new(logItem.Message, logItem.MemberName, logItem.SourceLineNumber, logItem.Args);

            string keys = JsonConvert.SerializeObject(customLogKeys);
            string data = JsonConvert.SerializeObject(customLogData);

            if (logItem.Exception is null)
            {
                if (logItem.EventId is null)
                {
                    logItem.Logger.Log(logItem.LogLevel, "{SourceContext}{ApplicationName}{LogLevel}{LogType}{Timestamp}{Keys}{Data}{CurrentStep}", logItem.SourceContext, ApplicationName, logItem.LogLevel.ToString(), logItem.LogType.ToString(), logItem.CurrentTime, keys, data, logItem.CurrentStep);
                }
                else
                {
                    logItem.Logger.Log(logItem.LogLevel, logItem.EventId.Value, "{SourceContext}{ApplicationName}{LogLevel}{LogType}{Timestamp}{Keys}{Data}{CurrentStep}", logItem.SourceContext, ApplicationName, logItem.LogLevel.ToString(), logItem.LogType.ToString(), logItem.CurrentTime, keys, data, logItem.CurrentStep);
                }
            }
            else
            {
                if (logItem.EventId is null)
                {
                    logItem.Logger.Log(logItem.LogLevel, logItem.Exception, "{SourceContext}{ApplicationName}{LogLevel}{LogType}{Timestamp}{Keys}{Data}{CurrentStep}{ExceptionDetail}", logItem.SourceContext, ApplicationName, logItem.LogLevel.ToString(), logItem.LogType.ToString(), logItem.CurrentTime, keys, data, logItem.CurrentStep, logItem.Exception);
                }
                else
                {
                    logItem.Logger.Log(logItem.LogLevel, logItem.EventId.Value, logItem.Exception, "{SourceContext}{ApplicationName}{LogLevel}{LogType}{Timestamp}{Keys}{Data}{CurrentStep}{ExceptionDetail}", logItem.SourceContext, ApplicationName.ToString(), logItem.LogType.ToString(), logItem.LogLevel, keys, logItem.CurrentTime, data, logItem.CurrentStep, logItem.Exception);
                }
            }
        }

        #region Helper

        private static dynamic GetDynamicObject(Dictionary<string, object> properties)
        {
            return new MyDynObject(properties);
        }

        private sealed class MyDynObject : DynamicObject
        {
            private readonly Dictionary<string, object> _properties;

            public MyDynObject(Dictionary<string, object> properties)
            {
                _properties = properties;
            }

            public override IEnumerable<string> GetDynamicMemberNames()
            {
                return _properties.Keys;
            }

            public override bool TryGetMember(GetMemberBinder binder, out object? result)
            {
                if (_properties.ContainsKey(binder.Name))
                {
                    result = _properties[binder.Name];
                    return true;
                }
                else
                {
                    result = null;
                    return false;
                }
            }

            public override bool TrySetMember(SetMemberBinder binder, object? value)
            {
                if (_properties.ContainsKey(binder.Name) && value != null)
                {
                    _properties[binder.Name] = value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        #endregion

        public void Finish()
        {
            if (EnableScopeKeys)
            {
                LogHistory.LogItems.ForEach(h => InjectLog(h));
            }

            LogHistory.LogItems = new();
        }
    }
}
