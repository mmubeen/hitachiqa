using Hsl.HitachiQA.Builder.Attributes;

namespace Hsl.HitachiQA.Builder.Commands.Info; 

public class Arguments : ArgumentsBase
{
    public Arguments(string[] args) : base(args) { }

    [ParameterKey("version-file-destination", "o")]
    [ParameterDescription(
        "Specifies the destination file path to save the version information. \n" +
        "Must be a valid path and a JSON file.")]
    public string VersionDestination { get; set; }
}