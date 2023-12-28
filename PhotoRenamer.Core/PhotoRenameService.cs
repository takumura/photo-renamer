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
    readonly string[] excludePatterns = [@"**/*_IMG_*"];

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

        var renameInfoList = files
            .AsParallel()
            .WithDegreeOfParallelism(maxDegreeOfParallelism)
            .Select(filePath => new RenameInfo()
            {
                OrginalFilePath = filePath,
                Name = Path.GetFileNameWithoutExtension(filePath),
                Ext = Path.GetExtension(filePath),
                CreationTime = File.GetCreationTime(filePath)
            });
            //.ToImmutableArray(); // performance issue wasn't happened when keep using ParallelQuery<RenameInfo>

        // create date folders
        var creationDates = renameInfoList
            .Select(x => x.CreationTime.ToString("yyyy-MM-dd"))
            .Distinct()
            .ToImmutableArray(); // need to evaluate the parallelQuery result, performance issue happened on next Parallel.ForEach method without evaluation

        Parallel.ForEach(
            creationDates,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            creationDate =>
            {
                CreateDateFolder(baseDirectory, creationDate);
            });

        Parallel.ForEach(
            renameInfoList,
            new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism },
            renameInfo =>
            {
                var checkLivePhotoResult = CheckLivePhoto(renameInfoList, renameInfo);
                if (checkLivePhotoResult.exists)
                {
                    ProcessRename(baseDirectory, categoryName, renameInfo, checkLivePhotoResult.livePhotoBaseRenameInfo!);
                }
                else
                {
                    ProcessRename(baseDirectory, categoryName, renameInfo);
                }
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

    private (bool exists, RenameInfo? livePhotoBaseRenameInfo) CheckLivePhoto(IEnumerable<RenameInfo> renameInfoList, RenameInfo renameInfo)
    {
        // currently, only the target file extention is ".mov".
        if (renameInfo.Ext != ".mov") return (false, null);

        // if it's live photo movie file, there will be a picture file which has same name with other extention (jpeg or heic).
        var livePhotoFile = renameInfoList.Where(x => x.Ext != ".mov" && x.Name == renameInfo.Name).SingleOrDefault();
        return livePhotoFile == null
            ? (false, null)
            : (true, livePhotoFile);
    }

    private void ProcessRename(string baseDirectory, string categoryName, RenameInfo renameInfo)
    {
        var creationYMD = renameInfo.CreationTime.ToString("yyyyMMdd");
        var creationHMS = renameInfo.CreationTime.ToString("HHmmss");
        var newFileName = $"{creationYMD}_{creationHMS}_{categoryName}_{renameInfo.Name}{renameInfo.Ext}";

        var creationKebabYMD = renameInfo.CreationTime.ToString("yyyy-MM-dd");
        logger.LogTrace(Chalk.BrightCyan + $"Rename file: {renameInfo.Name}{renameInfo.Ext} to {creationKebabYMD}\\{newFileName}");
        if (!isPreview)
        {
            var newFilePath = $"{baseDirectory}\\{creationKebabYMD}\\{newFileName}";
            File.Move(renameInfo.OrginalFilePath, newFilePath);
        }
    }

    private void ProcessRename(string baseDirectory, string categoryName, RenameInfo renameInfo, RenameInfo livePhotoBaseRenameInfo)
    {
        var creationYMD = livePhotoBaseRenameInfo.CreationTime.ToString("yyyyMMdd");
        var creationHMS = livePhotoBaseRenameInfo.CreationTime.ToString("HHmmss");
        var newFileName = $"{creationYMD}_{creationHMS}_{categoryName}_{renameInfo.Name}{renameInfo.Ext}";

        var creationKebabYMD = livePhotoBaseRenameInfo.CreationTime.ToString("yyyy-MM-dd");
        logger.LogTrace(Chalk.BrightCyan + $"Rename live photo file: {renameInfo.Name}{renameInfo.Ext} to {creationKebabYMD}\\{newFileName}");
        if (!isPreview)
        {
            var newFilePath = $"{baseDirectory}\\{creationKebabYMD}\\{newFileName}";
            File.Move(renameInfo.OrginalFilePath, newFilePath);
        }
    }
}