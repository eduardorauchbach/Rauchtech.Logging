using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;

namespace Rauchtech.Logging
{
    public static class Initializer
    {
        #region Logging

        public static IServiceCollection RegisterCustomLogs(this IServiceCollection services)
        {
            services.RegisterCustomLogModule()
                    .ConfigureSerilog();

            return services;
        }

        private static IServiceCollection ConfigureSerilog(this IServiceCollection services)
        {
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            var telemetryInitializer = new UserIdTelemetryInitializer(Environment.GetEnvironmentVariable("ApplicationName") ?? "Anonymous");
            telemetryConfiguration.TelemetryInitializers.Add(telemetryInitializer);

            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Verbose()
                         .MinimumLevel.Override("Host.Startup", LogEventLevel.Warning)
                         .MinimumLevel.Override("Host.General", LogEventLevel.Warning)
                         .MinimumLevel.Override("Host.Triggers.Warmup", LogEventLevel.Warning)
                         .MinimumLevel.Override("Host.Results", LogEventLevel.Warning)
                         .MinimumLevel.Override("Host.Aggregator", LogEventLevel.Warning)
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                         .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Warning)
                         .MinimumLevel.Override("System", LogEventLevel.Warning)
                         .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
                         .Enrich.WithExceptionDetails()
                         .Enrich.FromLogContext()
                         .WriteTo.ApplicationInsights(
                             telemetryConfiguration,
                             TelemetryConverter.Events,
                             LogEventLevel.Verbose)
                         .CreateLogger();

            return services.AddLogging(configure => configure.AddSerilog(Log.Logger));
        }

        #endregion

        #region Telemetry Helper

        private class UserIdTelemetryInitializer : ITelemetryInitializer
        {
            private readonly string _userId;

            public UserIdTelemetryInitializer(string userId)
            {
                _userId = userId;
            }

            public void Initialize(ITelemetry telemetry)
            {
                telemetry.Context.User.Id = _userId;
            }
        }

        #endregion
    }
}
