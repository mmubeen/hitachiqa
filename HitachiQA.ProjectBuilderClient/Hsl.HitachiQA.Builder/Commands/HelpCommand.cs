using Hsl.HitachiQA.Builder.Attributes;
using Hsl.HitachiQA.Builder.Commands;
using System.Reflection;

[Command("help")]
public class HelpCommand : ICommand
{
    public Task RunAsync(string[] args)
    {
        Console.WriteLine(Help());
        return Task.CompletedTask;
    }

    public string Help()
    {
        var toolName = GetToolCommandName();
        var commandTypes = typeof(HelpCommand).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<CommandAttribute>() != null && t != typeof(HelpCommand));

        var helpText = $"Usage of {toolName}:\n\nAvailable Commands:\n";

        foreach (var type in commandTypes)
        {
            // Create an instance of the command (excluding HelpCommand)
            var commandInstance = (ICommand)Activator.CreateInstance(type);
            helpText += $"\n{commandInstance.Help().Trim()}\n";
        }

        // Add the HelpCommand's own help text
        helpText += $"\nCommand: help\n\nUsage:\n  {toolName} help\n\nDescription:\n  Displays this help message.";

        return helpText.Trim();
    }

    private static string GetToolCommandName()
    {
        var assembly = typeof(HelpCommand).Assembly;
        var att = assembly.GetCustomAttribute<AssemblyToolCommandNameAttribute>();
        return att?.ToolCommandName ?? "<toolName>";
    }
}
