// See https://aka.ms/new-console-template for more information
using CatalogManager;
using CatalogManager.Metadata;
using Database;
using Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;


var connectionString = "Server=orion;Database=OpenVid;Trusted_Connection=True;Encrypt=False;";
var contextBuilder = new DbContextOptionsBuilder<OpenVidContext>();
contextBuilder.UseSqlServer(connectionString);
//var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
var logger = new LoggerConfiguration().CreateLogger();
var db = new VideoRepository(new OpenVidContext(contextBuilder.Options));
var thumbService = new ThumbnailService(
    logger,
    Options.Create(new CatalogImportOptions()
    {
        BucketDirectory = "D:\\inetpub\\wwwbucket"
    }),
    db,
    new FFMpegStrategy(logger),
    contextBuilder.Options
);
var bookService = new FlickbookService(
    logger,
    Options.Create(new CatalogImportOptions()
    {
        BucketDirectory = "D:\\inetpub\\wwwbucket"
    }),
    db,
    new FFMpegStrategy(logger),
    contextBuilder.Options
);

var videoIds = db.GetAllVideos().Where(v => v.Id > 0).Select(v => v.Id).ToList();

Console.WriteLine($"Found {videoIds.Count} videos...");

foreach (var id in videoIds)
{
    Console.Write($"Generating images for {id}... ");
    var start = DateTime.Now;
    Console.Write($"Thumbnail... ");
    await thumbService.InvokeService(id);
    Console.Write($"Flickbook... ");
    await bookService.InvokeService(id);
    var duration = DateTime.Now - start;
    Console.WriteLine($"Completed in {duration.TotalSeconds}s");
}