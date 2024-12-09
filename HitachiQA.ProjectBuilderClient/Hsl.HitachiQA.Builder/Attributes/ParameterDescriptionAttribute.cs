namespace Hsl.HitachiQA.Builder.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ParameterDescriptionAttribute : Attribute
{
    public string Description { get; init; }

    public ParameterDescriptionAttribute(string desc)
    {
        Description = desc;
    }
}