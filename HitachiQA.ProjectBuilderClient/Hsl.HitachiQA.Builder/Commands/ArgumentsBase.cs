using Hsl.HitachiQA.Builder.Attributes;
using System.Reflection;

namespace Hsl.HitachiQA.Builder.Commands;

public abstract class ArgumentsBase
{
    protected ArgumentsBase(string[] args)
    {
        ParseArgs(args);
        ValidateRequiredProperties();
    }
    private void ParseArgs(string[] args)
    {
        var properties = this.GetType().GetProperties();

        for (int i = 0; i < args.Length; i++)
        {
            var argument = args[i];
            var value = i + 1 < args.Length ? args[i + 1] : null;

            foreach (var property in properties)
            {
                var keyAttribute = property.GetCustomAttribute<ParameterKeyAttribute>();
                if (keyAttribute != null && keyAttribute.Keys.Contains(argument.TrimStart('-')))
                {
                    property.SetValue(this, value);
                    i++; // Skip the value
                    break;
                }
            }
        }
    }


    private void ValidateRequiredProperties()
    {
        var requiredProps = GetType().GetProperties()
            .Where(prop => Attribute.IsDefined(prop, typeof(RequiredAttribute)));

        var missingProps = requiredProps
            .Where(prop =>
                prop.GetValue(this) == null ||
                (prop.GetValue(this) is string str && string.IsNullOrWhiteSpace(str)))
            .Select(prop =>
            {
                var keyAttribute = prop.GetCustomAttribute<ParameterKeyAttribute>();
                return keyAttribute != null && keyAttribute.Keys.Any()
                    ? string.Join(", ", keyAttribute.Keys.Select(k => $"--{k}"))
                    : $"--{prop.Name}";
            })
            .ToList();

        if (missingProps.Any())
        {
            throw new ArgumentException(
                $"\nThe following required arguments were not provided:\n{string.Join("\n", missingProps)}\n"
            );
        }
    }



    public override string ToString()
    {
        var props = GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(string) ||
                        p.PropertyType == typeof(int) ||
                        p.PropertyType == typeof(double) ||
                        p.PropertyType == typeof(float) ||
                        p.PropertyType == typeof(bool) ||
                        p.PropertyType == typeof(long) ||
                        Nullable.GetUnderlyingType(p.PropertyType) == typeof(int) ||
                        Nullable.GetUnderlyingType(p.PropertyType) == typeof(double) ||
                        Nullable.GetUnderlyingType(p.PropertyType) == typeof(float) ||
                        Nullable.GetUnderlyingType(p.PropertyType) == typeof(long));

        return string.Join("\n", props.Select(p => $"{p.Name}={p.GetValue(this)}"));
    }
}
