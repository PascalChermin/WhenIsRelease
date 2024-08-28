using System;
using System.IO;
using Hangfire;
using Hangfire.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using WhenIsRelease.Data.Models.Context;
using WhenIsRelease.Data.Scheduler;

namespace WhenIsRelease.Data
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "WhenIsRelease", Version = "V1" });
                c.DescribeAllEnumsAsStrings();
            });

            services.AddSingleton(new LoggerFactory().AddNLog());
            this.EnsureDb();

#if DEBUG
            NLog.LogManager.LoadConfiguration("nlog.debug.config");
#elif TEST
            NLog.LogManager.LoadConfiguration("nlog.test.config");
#else
            NLog.LogManager.LoadConfiguration("nlog.release.config");
#endif

            GlobalConfiguration.Configuration.UseNLogLogProvider();
            services.AddLogging();

            services.AddDbContext<GameContext>();
            services.AddSingleton<ISocial, Twitter>();
            services.AddScoped<IBusiness, GameBusiness>();
            services.AddScoped<IMaintenance, GameMaintenance>();

            services.AddDefaultIdentity<IdentityUser>()
                .AddRoles<IdentityRole>();

#if DEBUG
            // Use docker image by running "docker-compose up" in root directory
            services.AddHangfire(x => x.UseSqlServerStorage("Server=localhost,1435;Database=WhenIsRelease;User Id=sa;Password=yourStrong(!)Password"));
#else
            services.AddHangfire(x => x.UseSQLiteStorage($"Data Source={Environment.GetEnvironmentVariable("WhenIsReleaseHangfireDB")};"));
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            var options = new BackgroundJobServerOptions { WorkerCount = 1 };
            app.UseHangfireServer(options);

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "WhenIsRelease v1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var ctx = scope.ServiceProvider.GetService<GameContext>();
                ctx.Database.Migrate();
            }

            RecurringJob.AddOrUpdate<GameMaintenance>(m => m.UpdateDatabaseAsync(), Cron.Daily);
            RecurringJob.AddOrUpdate<GameMaintenance>(m => m.TweetReleasesOfTheDay(), Cron.Daily(7));
        }

        private void EnsureDb()
        {
            if (File.Exists(Environment.GetEnvironmentVariable("WhenIsReleaseLogDB")))
                return;

            using (SqliteConnection connection = new SqliteConnection($"Data Source={Environment.GetEnvironmentVariable("WhenIsReleaseLogDB")};"))
            using (SqliteCommand command = new SqliteCommand(
                @"CREATE TABLE [Log] (
                     ID INTEGER PRIMARY KEY,
                     MachineName TEXT,
                     Logged DATETIME NOT NULL,
                     Level TEXT NOT NULL,
                     Message TEXT NOT NULL,
                     Logger TEXT NULL,
                     Properties TEXT NULL,
                     Callsite TEXT NULL,
                     Exception TEXT NULL);",
                connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
