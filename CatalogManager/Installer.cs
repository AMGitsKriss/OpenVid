using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogManager
{
    public static class Installer
    {
        public static IServiceCollection CatalogManagerInstaller(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<CatalogImportOptions>(configuration.GetSection("Catalog"));

            services.AddScoped<ImportService>();

            return services;
        }
    }
}
