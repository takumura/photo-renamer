using Microsoft.Extensions.FileSystemGlobbing;

namespace PhotoRenamer.Core;

public class FileMatcher
{
    readonly Matcher matcher = new();

    public IEnumerable<string> GetResultsInFullPath(string dirPath, string[] includePatterns, string[]? excludePatterns = null)
    {
        // use new file globbing library
        // see https://docs.microsoft.com/ja-jp/dotnet/core/extensions/file-globbing
        matcher.AddIncludePatterns(includePatterns);
        if (excludePatterns != null)
        {
            matcher.AddExcludePatterns(excludePatterns);
        }

        var matchingFiles = matcher.GetResultsInFullPath(dirPath);
        return matchingFiles;
    }
}
