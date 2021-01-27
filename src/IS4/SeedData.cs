using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public class SeedData
{
    public static void EnsureSeedData(string cn)
    {
        var migrationAssembly = typeof(SeedData).GetTypeInfo().Assembly.GetName().Name;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOperationalDbContext((options) => {
                options.ConfigureDbContext = x => x.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
            })
            .AddConfigurationDbContext(options => {
                options.ConfigureDbContext = x => x.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
            })
            .AddDbContext<ApplicationDbContext>(options => {
                options.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
            })
            .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var persistedGrantContext = scope.ServiceProvider.GetService<PersistedGrantDbContext>();
            persistedGrantContext.Database.Migrate();

            var configurationContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
            configurationContext.Database.Migrate();
            EnsureSeedData(configurationContext);

            var applicationDbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            applicationDbContext.Database.Migrate();
            EnsureUsers(scope);
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

    private static void EnsureUsers(IServiceScope scope)
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var alice = userManager.FindByNameAsync("alice").Result;

        if (alice == null)
        {
            alice = new IdentityUser
            {
                UserName = "alice",
                Email = "alice@email.com",
                EmailConfirmed = true
            };

            var result = userManager.CreateAsync(alice, "Password123$").Result;
            if(!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userManager.AddClaimsAsync(alice, new Claim[] {
                new Claim(JwtClaimTypes.Name, "Alice Smith"), 
                new Claim(JwtClaimTypes.GivenName, "Alice")
            }).Result;

            if(!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("alice created");
        }
        else
        {
            Log.Debug("alice already exists");
        }
    }
}
