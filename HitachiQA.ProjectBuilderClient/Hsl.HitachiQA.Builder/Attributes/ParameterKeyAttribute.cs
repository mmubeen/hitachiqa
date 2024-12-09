namespace Hsl.HitachiQA.Builder.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ParameterKeyAttribute : Attribute
{
    public string[] Keys { get; }

    public ParameterKeyAttribute(params string[] keys)
    {
        Keys = keys;
    }
}
