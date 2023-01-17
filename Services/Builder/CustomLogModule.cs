using Rauchtech.Logging.Services.Code;
using Microsoft.Extensions.DependencyInjection;

namespace Rauchtech.Logging
{
    internal static class CustomLogModule
    {
        public static IServiceCollection RegisterCustomLogModule(this IServiceCollection services)
        {
            services.AddScoped(typeof(ICustomLog<>), typeof(CustomLog<>));
            services.AddScoped<ICustomLogFactory, CustomLogFactory>();

            return services;
        }
    }
}
