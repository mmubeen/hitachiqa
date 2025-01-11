using System;
using System.IO;

namespace Hsl.HitachiQA.Builder.Tools;

public class FileCoppier
{
    private readonly string _projectDirectory;
    private readonly string _driver;
    private readonly string _assetsDirectory;

    public FileCoppier(string projectDirectory, string driver)
    {
        _projectDirectory = projectDirectory;
        _driver = driver;
        _assetsDirectory = Path.Combine(AppContext.BaseDirectory, "Build");
    }

    public void CopyFiles(string projectName, string targetHost)
    {
        // Create directories
        CreateDirectory(Path.Combine(_projectDirectory, "Features"));
        CreateDirectory(Path.Combine(_projectDirectory, "StepDefinitions"));
        CreateDirectory(Path.Combine(_projectDirectory, "Pages"));

        // Copy shared assets
        CopyFile("default.runsettings", _projectDirectory);
        CopyFile("ImplicitUsings.cs", _projectDirectory);
        CopyFile("Nuget.config", _projectDirectory);
        CopyFile("reqnroll.json", _projectDirectory);
        CopyFile("HsalSearch.feature", Path.Combine(_projectDirectory, "Features"));

        // Copy driver-specific assets
        if (_driver.Equals("playwright", StringComparison.OrdinalIgnoreCase))
        {
            CopyFile("playwright/HsalSearchSteps.cs", Path.Combine(_projectDirectory, "StepDefinitions"));
            CopyFile("playwright/HsalHome.cs", Path.Combine(_projectDirectory, "Pages"));
            CopyFile("playwright/appsettings.json", _projectDirectory);
        }
        else
        {
            CopyFile("selenium/HsalSearchSteps.cs", Path.Combine(_projectDirectory, "StepDefinitions"));
            CopyFile("selenium/HsalHome.cs", Path.Combine(_projectDirectory, "Pages"));
            CopyFile("selenium/appsettings.json", _projectDirectory);
        }

        // Apply transformations
        var fileNames = new[]
        {
            Path.Combine(_projectDirectory, "StepDefinitions", "HsalSearchSteps.cs"),
            Path.Combine(_projectDirectory, "Pages", "HsalHome.cs"),
            Path.Combine(_projectDirectory, "default.runsettings"),
            Path.Combine(_projectDirectory, $"{projectName}.csproj")
        };

        foreach (var fileName in fileNames)
        {
            if (File.Exists(fileName))
            {
                TransformFileContent(fileName, projectName, targetHost);
            }
        }

        Console.WriteLine("File copying and transformations completed.");
    }

    private void CreateDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            Console.WriteLine($"Created directory: {path}");
        }
    }

    private void CopyFile(string sourceRelativePath, string destinationDirectory)
    {
        var sourcePath = Path.Combine(_assetsDirectory, sourceRelativePath);
        var destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(sourceRelativePath));

        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, destinationPath, overwrite: true);
            Console.WriteLine($"Copied: {sourcePath} -> {destinationPath}");
        }
        else
        {
            Console.WriteLine($"File not found: {sourcePath}");
        }
    }

    private void TransformFileContent(string fileName, string projectName, string targetHost)
    {
        var fileContent = File.ReadAllText(fileName);

        // Replace placeholders
        fileContent = fileContent.Replace("{{ProjectName}}", projectName);
        fileContent = fileContent.Replace("{{TargetHost}}", targetHost);

        // Insert framework-specific transformations
        fileContent = fileContent.Replace("</TargetFramework>", @"</TargetFramework>
	<RunSettingsFilePath>$(MSBuildProjectDirectory)\default.runsettings</RunSettingsFilePath>");

        fileContent = fileContent.Replace("</Project>", @"  <ItemGroup>
    <None Update=""appsettings.json"">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </None>
  </ItemGroup>

</Project>");

        File.WriteAllText(fileName, fileContent);
        Console.WriteLine($"Transformed file: {fileName}");
    }
}
