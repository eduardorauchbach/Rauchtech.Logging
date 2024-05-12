using RauchTech.Logging.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace RauchTech.Logging.Builder
{
    public static class CustomLogModule
    {
        public static IServiceCollection RegisterCustomLogs(this IServiceCollection services)
        {
            services.AddLogging();

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddScoped<ICustomLogFactory, CustomLogFactory>();
            services.AddScoped(typeof(ICustomLog<>), typeof(CustomLog<>));            

            return services;
        }
    }
}
