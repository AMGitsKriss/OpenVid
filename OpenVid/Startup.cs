using Database;
using Database.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Search;
using Search.Filters;
using TagCache;
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

            services.AddDbContext<OpenVidContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultDatabase")));
            services.AddScoped<IVideoRepository, VideoRepository>();
            services.AddScoped<PaginatedSearch, PaginatedSearch>();
            services.AddScoped<IVideoManager, VideoManager>();
            services.AddScoped<UrlResolver, UrlResolver>();

            services
              .AddScoped<IFilter, GeneralFilter>()
              .AddScoped<IFilter, TagFilter>()
              .AddScoped<IFilter, MetaFilter>()
              .AddScoped<IFilter, ExtensionFilter>()
              .AddScoped<IFilter, RatingFilter>()
              .AddScoped<IFilter, RatingOrSaferFilter>()
              .AddScoped<IFilter, RatingOrRiskierFilter>()
              .AddScoped<IFilter, MinDurationFilter>()
              .AddScoped<IFilter, MaxDurationFilter>();

            services.TagCacheInstaller();
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

                using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
                {
                    var context = serviceScope.ServiceProvider.GetRequiredService<OpenVidContext>();
                    context.Database.EnsureCreated();
                }
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
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
