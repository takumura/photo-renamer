namespace PhotoRenamer.Core;

public interface IPhotoRenameService
{
    void Run(string input, string category, bool verbose = false, bool preview = false);
}
