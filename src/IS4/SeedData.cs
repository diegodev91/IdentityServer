using System.Linq;
using System.Reflection;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public class SeedData
{
    public static void EnsureSeedData(string cn)
    {
        var migrationAssembly = typeof(SeedData).GetTypeInfo().Assembly.GetName().Name;

        var services = new ServiceCollection();
        services.AddOperationalDbContext((options) => {
                options.ConfigureDbContext = x => x.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
            })
            .AddConfigurationDbContext(options => {
                options.ConfigureDbContext = x => x.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
            });
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
            var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            context.Database.Migrate();
            EnsureSeedData(context);
        }
    }

    private static void EnsureSeedData(ConfigurationDbContext context)
        {
            Log.Debug("Seeding database...");

            if (!context.Clients.Any())
            {
                Log.Debug("Clients being populated");
                foreach (var client in Config.GetClients())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Log.Debug("IdentityResources being populated");
                foreach (var resource in Config.GetIdentityResources())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("IdentityResources already populated");
            }

            if (!context.ApiResources.Any())
            {
                Log.Debug("ApiResources being populated");
                foreach (var resource in Config.GetApis())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiResources already populated");
            }

            Log.Debug("Done seeding database.");
        }
}
