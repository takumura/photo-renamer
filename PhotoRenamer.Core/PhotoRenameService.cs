using System.Collections.Immutable;
using Kokuban;
using Microsoft.Extensions.Logging;

namespace PhotoRenamer.Core;

public class PhotoRenameService : IPhotoRenameService
{
    readonly ILogger<IPhotoRenameService> logger;
    // use 75% of logical processors for parallel.ForEach and parallel.ForEachAsync
    // https://learn.microsoft.com/ja-jp/dotnet/api/system.environment.processorcount
    readonly int maxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.75));
    readonly string[] includePatterns = ["**/*.arw", "**/*.heic", "**/*.jpg", "**/*.mov", "**/*.mp4", "**/*.png"];
    readonly string[] excludePatterns = [@"**/*_*"];

    bool isPreview;

    public PhotoRenameService(ILogger<IPhotoRenameService> logger)
    {
        this.logger = logger;
    }

    public void Run(string input, string category, bool verbose = false, bool preview = false)
    {
        // setup field variables
        isPreview = preview;

        // set base directory
        var baseDirectory = string.IsNullOrEmpty(input)
            ? Directory.GetCurrentDirectory()
            : input;
        //// test directory
        //baseDirectory = "[target directory path]";

        // set category
        var di = new DirectoryInfo(baseDirectory);
        var directoryName = di.Name;
        var categoryName = string.IsNullOrEmpty(category)
            ? directoryName
            : category;

        if (isPreview)
        {
            logger.LogInformation($"Preview renaming under {baseDirectory} folder");
        }
        else
        {
            logger.LogInformation($"Start renaming under {baseDirectory} folder");
        }

        var files = SearchTargetDirectory(baseDirectory, di, categoryName);
        RenameFiles(baseDirectory, files, categoryName);
    }

    private ImmutableArray<string> SearchTargetDirectory(string baseDirectory, DirectoryInfo di, string categoryName)
    {
        var includePatternString = string.Join(", ", includePatterns);
        var excludePatternString = string.Join(", ", excludePatterns);
        logger.LogTrace(Chalk.BrightCyan + $"list up rename target file(s) from: {baseDirectory}, includePatterns: {includePatternString}, excludePatterns: {excludePatternString}");
        var result = new FileMatcher().GetResultsInFullPath(baseDirectory, includePatterns, excludePatterns);

        //// check target files
        //foreach ( var path in result )
        //{
        //    logger.LogTrace(Chalk.BrightCyan + $"{path}");
        //}

        return result.ToImmutableArray();
    }

    private void RenameFiles(string baseDirectory, ImmutableArray<string> files, string categoryName)
    {
        if (files.Length == 0)
        {
            logger.LogTrace(Chalk.BrightCyan + $"no file to process, skip renaming");
            return;
        }

        logger.LogTrace(Chalk.BrightCyan + $"set MaxDegreeOfParallelism to {maxDegreeOfParallelism}");

        var detailedFileList = files
            .AsParallel()
            .WithDegreeOfParallelism(maxDegreeOfParallelism)
            .Select(filePath => (
                orginalFilePath: filePath,
                Name: Path.GetFileNameWithoutExtension(filePath),
                ext: Path.GetExtension(filePath),
                creationTime: File.GetCreationTime(filePath)
            ));

        // create date folders
        var creationDates = detailedFileList
            .Select(x => x.creationTime.ToString("yyyy-MM-dd"))
            .Distinct()
            .ToImmutableArray();

        Parallel.ForEach(
            creationDates,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            creationDate =>
            {
                CreateDateFolder(baseDirectory, creationDate);
            });

        Parallel.ForEach(
            detailedFileList,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            fileDetail =>
            {
                ProcessRename(baseDirectory, fileDetail, categoryName);
            });
    }

    private void CreateDateFolder(string baseDirectory, string creationDate)
    {
        var targetDirectory = $"{baseDirectory}\\{creationDate}";
        if (!Directory.Exists(targetDirectory))
        {
            logger.LogTrace($"Create new directory: {targetDirectory}");
            if (!isPreview)
            {
                Directory.CreateDirectory(targetDirectory);
            }
        }
    }

    private void ProcessRename(string baseDirectory, (string orginalFilePath, string name, string ext, DateTime creationTime) fileDetail, string categoryName)
    {
        var creationYMD = fileDetail.creationTime.ToString("yyyyMMdd");
        var creationHMS = fileDetail.creationTime.ToString("HHmmss");
        var newFileName = $"{creationYMD}_{creationHMS}_{categoryName}_{fileDetail.name}{fileDetail.ext}";

        var creationKebabYMD = fileDetail.creationTime.ToString("yyyy-MM-dd");
        logger.LogTrace(Chalk.BrightCyan + $"Rename file: {fileDetail.name}{fileDetail.ext} to {creationKebabYMD}\\{newFileName}");
        if (!isPreview)
        {
            var newFilePath = $"{baseDirectory}\\{creationKebabYMD}\\{newFileName}";
            File.Move(fileDetail.orginalFilePath, newFilePath);
        }
    }
}