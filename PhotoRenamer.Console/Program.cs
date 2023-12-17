using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PhotoRenamer.Console;
using PhotoRenamer.Core;
using ZLogger;

var builder = ConsoleApp.CreateBuilder(args);
builder.ConfigureServices((ctx, services) =>
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddZLoggerConsole();

        // bad manner but reviewing args and finding verbose options for rename/preview 
        if (args.Any(x => x == "-v" || x == "--verbose"))
        {
            logging.SetMinimumLevel(LogLevel.Trace);
        }
        else
        {
            logging.SetMinimumLevel(LogLevel.Information);
        }
    });

    services.AddSingleton<IPhotoRenameService, PhotoRenameService>();
});

var app = builder.Build();
app.AddCommands<PhotoRenamerCommands>();
app.Run();
