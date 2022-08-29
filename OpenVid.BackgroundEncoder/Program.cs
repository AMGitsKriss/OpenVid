using CatalogManager;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;

namespace OpenVid.BackgroundEncoder
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = LoadServiceCollection();

            var encoder = serviceProvider.GetService<EncoderContainer>();
            //var sleepSeconds = 2 * 60 * 1000;

            //while (true)
            //{
            encoder.Start();

            //Thread.Sleep(sleepSeconds);
            //}
        }
        static ServiceProvider LoadServiceCollection()
        {
            var configuration = LoadConfiguration();
            return new ServiceCollection()
                .AddOptions()
                .AddSingleton<EncoderContainer>()
                .AddDbContext<OpenVidContext>(o => o.UseSqlServer(configuration.GetConnectionString("DefaultDatabase")))
                .AddScoped<IVideoRepository, VideoRepository>()
                .CatalogManagerInstaller(configuration)
                .BuildServiceProvider();
        }

        static IConfigurationRoot LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();

        }
    }
}
