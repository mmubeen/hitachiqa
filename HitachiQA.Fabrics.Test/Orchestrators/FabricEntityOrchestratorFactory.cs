using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Fabrics.Test.Entities.Enum;
using Reqnroll.BoDi;
using System.Reflection;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class FabricEntityOrchestratorFactory
{
    private readonly IObjectContainer _objectContainer;

    public FabricEntityOrchestratorFactory(IObjectContainer objectContainer)
    {
        _objectContainer = objectContainer;
    }

    public EntityOrchestratorBase<T> GetForEntity<T>()
    {
        return (EntityOrchestratorBase<T>)GetForEntity(typeof(T));
    }

    public EntityOrchestratorBase GetForEntity(Type entityType)
    {
        var targetBase = typeof(EntityOrchestratorBase<>).MakeGenericType(entityType);


        // Find a registered type that implements EntityOrchestratorBase<T>
        var orchestratorType = typeof(FabricEntityOrchestratorFactory)
            .Assembly
            .GetTypes()
            .FirstOrDefault(t =>
                !t.IsAbstract &&
                 targetBase.IsAssignableFrom(t));

        if (orchestratorType == null)
            throw new InvalidOperationException($"No orchestrator found for entity type {entityType.FullName}");

        return (EntityOrchestratorBase)_objectContainer.Resolve(orchestratorType);
    }

    public EntityOrchestratorBase GetForEntity(FabricEntityTypeEnum type)
    {
        // Get the Type from the FabricEntity attribute
        var entityType = GetEntityTypeFromEnum(type);

        // Create and return the orchestrator
        return GetForEntity(entityType);
    }

    public T Get<T>() where T : EntityOrchestratorBase<T>
    {
        return _objectContainer.Resolve<T>();
    }

    private Type GetEntityTypeFromEnum(FabricEntityTypeEnum type)
    {
        var fieldInfo = typeof(FabricEntityTypeEnum).GetField(type.ToString());

        if (fieldInfo == null)
        {
            throw new ArgumentException($"Invalid enum value: {type}");
        }

        var attribute = fieldInfo.GetCustomAttribute<FabricEntityAttribute>();

        if (attribute == null)
        {
            throw new InvalidOperationException($"FabricEntity attribute not found on {type}");
        }

        return attribute.FabricEntityType;
    }
}
