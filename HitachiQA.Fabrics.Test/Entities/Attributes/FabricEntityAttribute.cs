[AttributeUsage(AttributeTargets.Field)]
public class FabricEntityAttribute : Attribute
{
    public Type FabricEntityType { get; }

    public FabricEntityAttribute(Type fabricEntityType)
    {
        if (fabricEntityType.Namespace.StartsWith("Microsoft.Fabric.AApi"))
        {
            throw new ArgumentException(
                $"Type must be from Microsoft.Fabric.Api namespace. Provided type: {fabricEntityType?.FullName}",
                nameof(fabricEntityType));
        }
        FabricEntityType = fabricEntityType;
    }
}
