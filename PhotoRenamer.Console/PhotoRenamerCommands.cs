using Kokuban;
using Microsoft.Extensions.Logging;
using PhotoRenamer.Core;

namespace PhotoRenamer.Console;

public class PhotoRenamerCommands : ConsoleAppBase
{
    readonly ILogger<PhotoRenamerCommands> logger;
    readonly IPhotoRenameService service;

    public PhotoRenamerCommands(ILogger<PhotoRenamerCommands> logger, IPhotoRenameService service)
    {
        this.logger = logger;
        this.service = service;
    }

    [RootCommand]
    public void Rename(
        [Option("i", "Optional. Set target directry(full path) which has photos to rename.")] string input = "",
        [Option("c", "Optional. Set category string aiming to identify the instruments taking photos. If this option is not set, current directly name is automatically used.")] string category = "",
        [Option("v", "Optional. Set output to verbose messages.")] bool verbose = false)
    {
        try
        {
            logger.LogTrace(Chalk.BrightCyan + "Start rename");
            service.Run(input, category, verbose);

            if (verbose)
            {
                logger.LogTrace(Chalk.BrightCyan + "Press any key to close the window...");
                System.Console.ReadLine();
            }
        }
        catch (Exception e)
        {
            logger.LogError(Chalk.Red + e.Message);
        }

    }

    [Command("preview", "Preview the rename process and display the expected result")]
    public void Preview(
        [Option("i", "Optional. Set target directry(full path) which has photos to rename.")] string input = "",
        [Option("c", "Optional. Set category string aiming to identify the instruments taking photos. If this option is not set, current directly name is automatically used.")] string category = "",
        [Option("v", "Optional. Set output to verbose messages.")] bool verbose = false)
    {
        try
        {
            logger.LogTrace(Chalk.BrightCyan + "Start preview");
            service.Run(input, category, verbose, true);

            if (verbose)
            {
                logger.LogTrace(Chalk.BrightCyan + "Press any key to close the window...");
                System.Console.ReadLine();
            }
        }
        catch (Exception e)
        {
            logger.LogError(Chalk.Red + e.Message);
        }
    }
}
