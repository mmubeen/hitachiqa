using System;
using System.IO;
using Hsl.HitachiQA.Builder.Attributes;
using Hsl.HitachiQA.Builder.Tools;

namespace Hsl.HitachiQA.Builder.Commands.Info;

[Command("info")]
public class InfoCommand : ICommand
{
    public async Task RunAsync(string[] args)
    {
        var arguments = new Arguments(args);

        // Validate the VersionDestination parameter
        ValidateVersionDestination(arguments.VersionDestination);

        var versionInfo = await VersionInfo.GetVersionInfo();

        if (!string.IsNullOrWhiteSpace(arguments.VersionDestination))
        {
            versionInfo.CopyTo(arguments.VersionDestination);
        }

        Console.WriteLine(versionInfo.ToString());
    }

    public string Help()
    {
        return $@"
Command: info

    Usage:
      <ToolCommandName> info [OPTIONS]

    {ICommand.GenerateHelp<Arguments>()}

    Description:
      Displays build-related information, such as supported frameworks and defaults.
      If the optional --version-file-destination (-o) parameter is provided, the version information
      will be saved to the specified file.";
    }

    private void ValidateVersionDestination(string versionDestination)
    {
        if (string.IsNullOrWhiteSpace(versionDestination))
        {
            return; // No destination provided, so no validation needed
        }

        // Check if the path ends with .json
        if (!versionDestination.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("The version file destination must have a .json extension.");
        }

        // Check if the path is valid
        try
        {
            string fullPath = Path.GetFullPath(versionDestination);

            // Ensure the directory exists
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                throw new ArgumentException($"The directory '{directory}' does not exist.");
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid version file destination path: {ex.Message}");
        }
    }
}
