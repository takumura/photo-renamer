namespace PhotoRenamer
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public interface IPhotoRenameService
    {
        void Run(CommandLineOptions options);
    }

    public class PhotoRenameService : IPhotoRenameService
    {
        readonly ILogger<IPhotoRenameService> _logger;
        readonly string[] extensions = new string[]
        {
            ".jpg",
            ".png",
            ".heic",
            ".mov",
            ".mp4",
            ".arw"
        };
        readonly List<string> _processedFiles = new List<string>();
        bool _isPreview;

        public PhotoRenameService(ILogger<IPhotoRenameService> logger)
        {
            _logger = logger;
        }

        public void Run(CommandLineOptions options)
        {
            // setup field variables
            _isPreview = options.Preview;

            var baseDirectory = !string.IsNullOrEmpty(options.InputDirectry)
                ? options.InputDirectry
                : Directory.GetCurrentDirectory();

            //// test directory
            //baseDirectory = "[target directory path]";

            var di = new DirectoryInfo(baseDirectory);
            var directoryName = di.Name;
            var categoryName = !string.IsNullOrEmpty(options.Category)
                ? options.Category
                : directoryName;

            _logger.LogInformation($"Base directory: {baseDirectory}");
            SearchDirectory(baseDirectory, di, categoryName);
        }

        void SearchDirectory(string baseDirectory, DirectoryInfo di, string categoryName)
        {
            _logger.LogInformation("Start renaming...");

            var targetDirectories = di.EnumerateDirectories();
            if (targetDirectories != null && targetDirectories.Count() > 0)
            {
                foreach (var recursiveDirectory in targetDirectories)
                {
                    SearchDirectory(baseDirectory, recursiveDirectory, categoryName);
                }
            }

            var fileInfoList = di.EnumerateFiles();
            foreach (var file in fileInfoList)
            {
                RenameFile(baseDirectory, di, categoryName, file.FullName);
            }
        }

        void RenameFile(string baseDirectory, DirectoryInfo di, string categoryName, string fileName)
        {
            FileInfo item = new FileInfo(fileName);
            if (_processedFiles.Any(x => x == item.FullName))
            {
                _logger.LogTrace($"[skip] File already processed: {item.FullName}");
                return;
            }

            if (!item.Exists)
            {
                _logger.LogTrace($"[skip] File not exist: {item.FullName}");
                return;
            }

            var extension = item.Extension.ToLower();
            if (!extensions.Any(x => x == extension))
            {
                _logger.LogTrace($"[skip] File extension is not a rename target: {item.FullName}");
                return;
            }

            var createdDate = item.CreationTime;
            var originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(item.Name);
            var newFileName = $"{createdDate.ToString("yyyyMMdd")}_{createdDate.ToString("HHmmss")}_{categoryName}_{item.Name}";

            var targetDirectory = $"{baseDirectory}/{createdDate.ToString("yyyy-MM-dd")}";
            if (!_isPreview)
            {
                if (!Directory.Exists(targetDirectory))
                {
                    _logger.LogTrace($"Create new directory: {targetDirectory}");
                    Directory.CreateDirectory(targetDirectory);
                }

                _logger.LogTrace($"Move file from {item.FullName} to {targetDirectory}/{newFileName}");
                item.MoveTo($"{targetDirectory}/{newFileName}");
            }
            else
            {
                _logger.LogInformation($"Move file from {item.FullName} to {targetDirectory}/{newFileName}");
            }
            _processedFiles.Add(item.FullName);

            // rename live photo file with same name and wrong extension
            var files = di.EnumerateFiles();
            var livePhotoFile = files.Where(x => item.Name != x.Name
                && originalFileNameWithoutExtension == Path.GetFileNameWithoutExtension(x.Name))
                .FirstOrDefault();

            if (livePhotoFile != null)
            {
                RenameLivePhotoFile(targetDirectory, livePhotoFile, categoryName, createdDate);
            }
        }

        void RenameLivePhotoFile(string targetDirectory, FileInfo livePhotoFile, string categoryName, DateTime createdDate)
        {
            var livePhotoNewFileName = $"{createdDate.ToString("yyyyMMdd")}_{createdDate.ToString("HHmmss")}_{categoryName}_{livePhotoFile.Name}";

            if (!_isPreview)
            {
                _logger.LogTrace($"Move live photo file from {livePhotoFile.FullName} to {targetDirectory}/{livePhotoNewFileName}");
                livePhotoFile.CreationTime = createdDate;
                livePhotoFile.MoveTo($"{targetDirectory}/{livePhotoNewFileName}");
            }
            else
            {
                _logger.LogInformation($"Move live photo file from {livePhotoFile.FullName} to {targetDirectory}/{livePhotoNewFileName}");
            }
            _processedFiles.Add(livePhotoFile.FullName);
        }
    }
}
