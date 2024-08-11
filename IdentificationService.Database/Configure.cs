using IdentificationService.Database.DataContext;
using IdentificationService.Database.DataSeeders;
using IdentificationService.Database.Interfaces;
using IdentificationService.Domain.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentificationService.Database
{
    public static class Configure
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services = ConfigureEntityFramework(services, configuration);

            services.AddScoped<IApplicationDbContext, ApplicationContext>();

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationContext>()
                .AddDefaultTokenProviders();

            return services;
        }
        private static IServiceCollection ConfigureEntityFramework(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentificationContext"), builder =>
                    {
                        builder.EnableRetryOnFailure(1, TimeSpan.FromSeconds(1), null);
                        builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    });

                options.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored));
            });

            #region Fournisseur d'info en cas d'exception DB.

            services.AddDatabaseDeveloperPageExceptionFilter();

            #endregion Fournisseur d'info en cas d'exception DB.

            return services;
        }

        public static async Task<IServiceCollection> ConfigureDatabaseForDevelopmentAsync(this IServiceCollection services, WebApplication app)
        {
            #region Fournisseur d'info en cas d'exception DB.

            app.UseDeveloperExceptionPage();
            app.UseMigrationsEndPoint();

            #endregion Fournisseur d'info en cas d'exception DB.

            #region Mise à jour du schéma de la DB.

            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider;

                var roleManager = service.GetRequiredService<RoleManager<ApplicationRole>>();
                var userManager = service.GetRequiredService<UserManager<ApplicationUser>>();
                var context = service.GetRequiredService<ApplicationContext>();

                context.Database.EnsureDeleted(); // Supprime la DB si elle existe.
                context.Database.EnsureCreated(); // Crée la DB si elle n'existe pas.
                await RoleSeeder.Seed(context, roleManager, userManager);
                context.SaveChanges();
            }


            #endregion Mise à jour du schéma de la DB.

            return services;
        }
    }
}
