namespace Fabrics.Test.Entities;

public class EntityInfo
{
    public string DisplayName { get; set; }
    public string Type { get; set; }
    public string FilePath { get; set; }
    public string ParentDirectory { get; set; }
    public List<string> AllFilesInDirectory { get; set; } = new List<string>();
}
