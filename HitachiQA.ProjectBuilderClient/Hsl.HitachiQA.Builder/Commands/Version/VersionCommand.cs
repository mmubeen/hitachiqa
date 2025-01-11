using System;
using System.IO;
using System.Reflection;
using Hsl.HitachiQA.Builder.Attributes;
using Hsl.HitachiQA.Builder.Tools;

namespace Hsl.HitachiQA.Builder.Commands.Info;

[Command("Version")]
public class VersionCommand : ICommand
{
    public Task RunAsync(string[] args)
    {
        var versionInfo = typeof(VersionCommand)
                            .Assembly
                            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                            .InformationalVersion ?? "Unknown version";
        var cleanVersion = versionInfo.Split('+')[0];
        Console.WriteLine("Version:");
        Console.WriteLine(cleanVersion);
        return Task.CompletedTask;
    }

    public string Help()
    {
        return $@"
Command: version

    Usage:
      <ToolCommandName> --version

    Description:
      Displays version of the tool installed";
    }

    
}
