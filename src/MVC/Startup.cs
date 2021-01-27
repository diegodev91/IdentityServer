using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVC.Services;

namespace MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           
            services.AddControllersWithViews();

            services.AddAuthentication(opt => {
                opt.DefaultScheme = "cookie";
                opt.DefaultChallengeScheme = "oicd";
            }).AddCookie("cookie")
            .AddOpenIdConnect("oicd", options => {
                options.Authority = Configuration["InteractiveServiceSettings:AuthorityUrl"];
                options.ClientId = Configuration["InteractiveServiceSettings:ClientId"];
                options.ClientSecret = Configuration["InteractiveServiceSettings:ClientSecret"];
                options.ResponseType = "code";
                options.UsePkce =  true;
                options.ResponseMode = "query";
                options.Scope.Add(Configuration["InteractiveServiceSettings:Scopes"]);

                options.SaveTokens = true;
            });

            services.Configure<IdentityServerSettings>(Configuration.GetSection("IdentityServerSettings"));
            services.AddSingleton<ITokenService, TokenService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting(); 

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
