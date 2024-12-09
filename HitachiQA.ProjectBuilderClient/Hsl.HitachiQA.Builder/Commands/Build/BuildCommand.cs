using Hsl.HitachiQA.Builder.Attributes;
using Hsl.HitachiQA.Builder.Tools;
using System.Diagnostics;


namespace Hsl.HitachiQA.Builder.Commands.Build;

[Command("build")]
public class BuildCommand : ICommand
{
    public async Task RunAsync(string[] args)
    {
        var arguments = new Arguments(args);
        var versionInfo = await VersionInfo.GetVersionInfo();
        var outputDir = Path.GetFullPath(arguments.OutputFolder);
        var projectName = arguments.ProjectName;
        var targetHost = arguments.TargetHost;
        Console.WriteLine("Executing Build Command..." + arguments);
        // Check if the MSTest template exists
        if (!await CheckTemplateExistsAsync("MSTest Test Project"))
        {
            Console.WriteLine("Error: The MSTest template does not exist.");
            Environment.Exit(1);
        }

        //create solution and project
        var solutionDir = await CreateSolutionAsync(outputDir, arguments.ProjectName, arguments.DotnetFramework);
        CleanDefaultFiles(solutionDir);

        // Add dependencies to the csproj (no build)
        Dependencies selectedDependencies = LoadDependencies(arguments, versionInfo);
        await AddDependenciesToSolution(solutionDir, selectedDependencies);

        // add all default files
        var fileCoppier = new FileCoppier(solutionDir, arguments.Driver);
        fileCoppier.CopyFiles(projectName, targetHost);
        Console.WriteLine("Build process completed.");
    }

    public string Help()
    {
        return $@"
Command: build

    Usage:
      <ToolCommandName> build [OPTIONS]

    {ICommand.GenerateHelp<Arguments>()}

    Description:
      Builds a project with the specified parameters.";
    }
    private async Task<bool> CheckTemplateExistsAsync(string templateName)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "new --list",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        await process.WaitForExitAsync();

        return output.Contains(templateName);
    }

    private async Task<string> CreateSolutionAsync(string outputDir, string projectName, string framework)
    {
        var proectDir = Path.Combine(outputDir, projectName);
        Console.WriteLine($"Creating solution and project: {projectName}");
        await RunProcessAsync(outputDir, "dotnet", $"new sln -n {projectName} -o ./{projectName}");
        await RunProcessAsync(outputDir, "dotnet", $"new mstest -n {projectName} -f {framework} -o ./{projectName}");
        await RunProcessAsync(proectDir, "dotnet", $"sln add ./{projectName}.csproj");
        Console.WriteLine($"Successfully created solution and project: {projectName}");
        return proectDir;

    }

    private static async Task RunProcessAsync(string workingDir, string fileName, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            Console.WriteLine($"Error executing command: {fileName} {arguments}");
            Console.WriteLine(error);
            Environment.Exit(process.ExitCode);
        }

        Console.WriteLine(output);
    }

    private void CleanDefaultFiles(string projDir)
    {
        string[] filesToDelete = { "UnitTest1.cs", "usings.cs" };

        foreach (var file in filesToDelete)
        {
            var filePath = Path.Combine(projDir, file);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted: {filePath}");
            }
            else
            {
                Console.WriteLine($"File not found: {filePath}");
            }
        }
    }

    private static async Task AddDependenciesToSolution(string solutionPath, Dependencies selectedDependencies)
    {
        foreach (var dependency in selectedDependencies)
        {
            var packageName = dependency.Key;
            var packageVersion = dependency.Value;

            Console.WriteLine($"Adding package: {packageName}, Version: {packageVersion}");

            // Run the dotnet add package command for each dependency
            await RunProcessAsync(solutionPath, "dotnet", $"add package {packageName} -n --version {packageVersion}");
        }

        Console.WriteLine("All dependencies added successfully.");
    }


    private static Dependencies LoadDependencies(Arguments arguments, VersionInfo versionInfo)
    {
        Console.WriteLine($"Loaded version info:\n{versionInfo}");

        Dependencies selectedDependencies = null;
        if (arguments.Driver.Equals("Selenium", StringComparison.OrdinalIgnoreCase))
        {
            selectedDependencies = versionInfo.Selenium?.Dependencies;
        }
        else if (arguments.Driver.Equals("Playwright", StringComparison.OrdinalIgnoreCase))
        {
            selectedDependencies = versionInfo.Playwright?.Dependencies;
        }
        else
        {
            Console.WriteLine($"Error: Unknown driver '{arguments.Driver}'. Supported values are 'Selenium' and 'Playwright'.");
            Environment.Exit(1);
        }

        if (selectedDependencies == null || !selectedDependencies.Any())
        {
            Console.WriteLine($"No dependencies found for driver '{arguments.Driver}'.");
            Environment.Exit(1);
        }

        return selectedDependencies;
    }
}
