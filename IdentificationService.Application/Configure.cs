using IdentificationService.Application.Interfaces;
using IdentificationService.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IdentificationService.Application
{
    public static class Configure
    {
        public static IServiceCollection ConfigureApplication(this IServiceCollection services)
        {
            services.AddScoped<IIdentityService, IdentityService>();

            return services;
        }
    }
}