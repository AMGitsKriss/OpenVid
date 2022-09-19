using CatalogManager.Encoder;
using CatalogManager.Metadata;
using CatalogManager.Segment;
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
            services.AddScoped<PlaybackService>();
            services.AddScoped<IMetadataStrategy, FFMpegStrategy>();
            services.AddScoped<IEncoderStrategy, HandbrakeStrategy>();
            services.AddScoped<ISegmenterStrategy, ShakaPackagerStrategy> ();

            return services;
        }
    }
}
