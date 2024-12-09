using Newtonsoft.Json.Linq;

namespace Hsl.HitachiQA.Builder.Tools;

public class VersionInfo
{
    public async static Task<VersionInfo> GetVersionInfo()
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Build", "versions.json");
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"versions.json not found in {filePath}");
        }

        string jsonContent = await File.ReadAllTextAsync(filePath);
        return JToken.Parse(jsonContent).ToObject<VersionInfo>(); 
    }
    public Driver Selenium { get; set; }
    public Driver Playwright { get; set; }
    public override string ToString()
    {
        return JToken.FromObject(this).ToString();
    }
    public void CopyTo(string destinationPath)
    {
        string filePath = Path.Combine(AppContext.BaseDirectory, "Build", "versions.json");
        File.Copy(filePath, destinationPath, overwrite: true);
    }
}

public class Driver
{
    public Dependencies Dependencies { get; set; }

}
public class Dependencies : Dictionary<string, string>
{
    
}
