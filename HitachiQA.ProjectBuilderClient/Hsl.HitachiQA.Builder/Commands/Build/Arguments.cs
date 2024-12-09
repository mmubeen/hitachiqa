using Hsl.HitachiQA.Builder.Attributes;
using System.Reflection;

namespace Hsl.HitachiQA.Builder.Commands.Build; 

public class Arguments : ArgumentsBase
{
    public Arguments(string[] args) : base(args) { }

    [ParameterKey("project-name", "n")]
    [Required]
    [ParameterDescription("The name of the project to build.")]
    public string ProjectName { get; set; }

    [ParameterKey("dotnet-framework", "f")]
    [ParameterDescription("target .NET framework (e.g; 'net8.0')")]
    public string DotnetFramework { get; set; } = "net8.0";

    [ParameterKey("target-host", "h")]
    [ParameterDescription("target host to open (website the browser will navigate to on open)")]
    public string TargetHost { get; set; } = "https://www.hitachi.us";

    [ParameterKey("output-folder", "o")]
    [Required]
    [ParameterDescription("The directory where this project will be created (will create it's own directory inside of this)")]
    public string OutputFolder { get; set; }

    [ParameterKey("driver", "d")]
    [Required]
    [ParameterDescription("The browser driver to use ('playwright', 'selenium')")]
    public string Driver { get; set; }
  

   
}