using System.Reflection;
using Hsl.HitachiQA.Builder.Attributes;
using Hsl.HitachiQA.Builder.Commands;




static class Program
{
    static string GetAssetPath()
    {
        var toolDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location);
        return Path.Combine(toolDirectory, "build");
    }
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Working Dir: " + Directory.GetCurrentDirectory());
            Console.WriteLine("Asset Dir: " + GetAssetPath());

            string commandName;
            string[] commandArgs;
            if (args.Length == 0)
            {
                commandName = null;
                commandArgs = null;
            }
            else if (args[0].ToLower().Trim('-')=="version")
            {
                commandName = "version";
                commandArgs = null;
            }
            else
            {
                commandName = args[0].ToLower();
                commandArgs = args.Skip(1).ToArray();
            }

            // Find all classes with the Command attribute
            var commandTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

            // Match the command name to a class
            var commandType = commandTypes.FirstOrDefault(t =>
                t.GetCustomAttribute<CommandAttribute>()?.Name == commandName);

            if (commandType != null)
            {
                // Create an instance of the command and execute it
                var commandInstance = (ICommand)Activator.CreateInstance(commandType);
                await commandInstance.RunAsync(commandArgs);
            }
            else
            {
                Console.WriteLine($"Unknown command '{commandName}'");
                var helpCommand = new HelpCommand();
                await helpCommand.RunAsync(null);
            }
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            if (Environment.UserInteractive && !Console.IsInputRedirected)
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("\nExiting...");
            }
        }
    }
}

