namespace Hsl.HitachiQA.Builder.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; }

    public CommandAttribute(string name)
    {
        Name = name.ToLower();
    }
}