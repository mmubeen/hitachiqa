using Hsl.HitachiQA.Builder.Attributes;
using System.Reflection;

namespace Hsl.HitachiQA.Builder.Commands;

public interface ICommand
{
    Task RunAsync(string[] args);
    string Help();

    public static string GenerateHelp<T>()
    {
        var argumentProperties = typeof(T).GetProperties()
            .Where(p => p.GetCustomAttribute<ParameterKeyAttribute>() != null);

        string helpMessage = "      Options:\n";

        foreach (var property in argumentProperties)
        {
            var keyAttribute = property.GetCustomAttribute<ParameterKeyAttribute>();
            var descriptionAttribute = property.GetCustomAttribute<ParameterDescriptionAttribute>();

            string keys = keyAttribute != null
                ? string.Join(", ", keyAttribute.Keys.Select(k => $"--{k}"))
                : "";

            string description = descriptionAttribute?.Description ?? "No description available.";

            // Format description with correct indentation
            string formattedDescription = FormatDescription(description, 32);

            helpMessage += $"        {keys,-25} {formattedDescription}\n";
        }

        return helpMessage;
    }

    private static string FormatDescription(string description, int firstLineIndent)
    {
        int maxWidth = 80; // Max width for each line
        int continuationIndent = firstLineIndent + 4; // Indent for continuation lines
        string continuationPadding = new string(' ', continuationIndent);

        var lines = new List<string>();
        var words = description.Split(' ');

        string currentLine = "";
        foreach (var word in words)
        {
            if ((currentLine + word).Length > maxWidth - firstLineIndent)
            {
                lines.Add(currentLine.Trim());
                currentLine = word + " ";
                firstLineIndent = continuationIndent; // Adjust for subsequent lines
            }
            else
            {
                currentLine += word + " ";
            }
        }

        if (!string.IsNullOrWhiteSpace(currentLine))
        {
            lines.Add(currentLine.Trim());
        }

        // Join lines with proper continuation padding
        return string.Join($"\n{continuationPadding}", lines);
    }

}
