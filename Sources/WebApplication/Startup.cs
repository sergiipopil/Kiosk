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
using Serilog;
using KioskBrains.Clients.AllegroPl;
using KioskBrains.Clients.YandexTranslate;
using KioskBrains.Server.Domain.Config;
using Microsoft.EntityFrameworkCore;
using KioskBrains.Server.Domain.Entities.DbStorage;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using WebApplication.Classes;
using KioskBrains.Clients.Ek4Car;


namespace WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration
            (configuration).CreateLogger();

            Configuration = configBuilder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AllegroPlClientSettings>(options => Configuration.GetSection("AllegroPlClientSettings").Bind(options));
            services.Configure<YandexTranslateClientSettings>(options => Configuration.GetSection("YandexTranslateClientSettings").Bind(options));
            services.Configure<Ek4CarClientSettings>(options => Configuration.GetSection("Ek4CarClientSettings").Bind(options));

            services.AddMvc();
            services.AddDbContext<KioskBrainsContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            
            services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic }));
            services.AddScoped<Ek4CarClient>();
            services.UpdateDatabase<KioskBrainsContext, DbInitializer>(services.BuildServiceProvider());
            services.AddControllersWithViews();
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
            //app.UseStaticFiles(new StaticFileOptions()
            //{
            //    OnPrepareResponse = ctx =>
            //    {
            //        ctx.Context.Response.Headers.Add("Cache-Control", "public,max-age=600");
            //    }
            //});
            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    //pattern: "{controller=Home}/{action=Index}/{*catchall}");
                    pattern: "{controller=Home}/{action=Index}/{param?}/{param10?}/{param2?}/{param3?}/{param4?}/{param5?}/{param6?}/{param7?}/{param8?}");
            });
        }
    }
}
