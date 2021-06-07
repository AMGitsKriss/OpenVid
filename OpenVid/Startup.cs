using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Search;
using Search.Filters;
using Upload;

namespace OpenVid
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
            services.AddControllersWithViews().AddRazorRuntimeCompilation();

            services.AddTransient<Videos, Videos>();
            services.AddSingleton<PaginatedSearch, PaginatedSearch>();
            services.AddSingleton<Save, Save>();

            services
              .AddSingleton<IFilter, GeneralFilter>()
              .AddSingleton<IFilter, TagFilter>()
              .AddSingleton<IFilter, MetaFilter>()
              .AddSingleton<IFilter, ExtensionFilter>()
              .AddSingleton<IFilter, RatingFilter>()
              .AddSingleton<IFilter, MinDurationFilter>()
              .AddSingleton<IFilter, MaxDurationFilter>();
            // TODO 0 The filters should be automatically loaded, not specified here

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            if (env.IsDevelopment())
            {
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    defaults: new { action = "Index" },
                    pattern: "{controller=Home}/{id?}");
            });
        }
    }
}
