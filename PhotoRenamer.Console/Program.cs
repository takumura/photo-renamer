using Cysharp.Text;
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
        logging.AddZLoggerConsole(options =>
        {
            // Tips: use PrepareUtf8 to achive better performance.
            var prefixFormat = ZString.PrepareUtf8<LogLevel, DateTime>("[{0}][{1}]");
            options.PrefixFormatter = (writer, info) => prefixFormat.FormatTo(ref writer, info.LogLevel, info.Timestamp.DateTime.ToLocalTime());
        });

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
