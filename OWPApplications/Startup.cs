using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OWPApplications.Data;
using OWPApplications.Utils;

namespace OWPApplications
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
            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = new List<CultureInfo> { new CultureInfo("en-US") };
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSession(options => options.IdleTimeout = TimeSpan.FromDays(2));

            services.AddSingleton<IConfiguration>(Configuration); //Inject Configuration
            services.AddSingleton<DbHandler>(); // Inject DBHandler
            services.AddSingleton<EmailHelper>(); // Inject DBHandler
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory logger)
        {
            app.UseRequestLocalization();
            app.UseDeveloperExceptionPage();

            logger.AddConsole(Configuration.GetSection("Logging"));
            logger.AddDebug();
            logger.AddFile("Logs/ts-{Date}.txt", LogLevel.Information);

            app.UseStaticFiles();
            app.UseMiddleware<DeChunkerMiddleware>();
            app.UseSession();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
