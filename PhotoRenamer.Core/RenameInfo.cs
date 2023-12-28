namespace PhotoRenamer.Core;

internal class RenameInfo
{
    public required string OrginalFilePath { get; set; }
    public required string Name { get; set; }
    public required string Ext {  get; set; }
    public DateTime CreationTime { get; set; }
}
