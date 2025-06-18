# HitachiQA Fabric Test Orchestrators Guide

This guide explains how to work with orchestrators and the `FabricEntityTypeEnum` in the HitachiQA Fabric Test framework.

## Overview

The orchestrator pattern in this framework provides a unified way to manage different types of Fabric entities (Workspace, DataPipeline, Notebook, etc.). The system consists of three main components:

1. `FabricEntityTypeEnum` - Defines the supported entity types
2. `EntityOrchestratorBase` - Base class for all orchestrators
3. `FabricEntityOrchestratorFactory` - Factory for creating entity-specific orchestrators

## FabricEntityTypeEnum

The `FabricEntityTypeEnum` is defined in `Entities/Enum/FabricEntityTypeEnum.cs` and maps enum values to their corresponding Fabric API entity types:

```csharp
public enum FabricEntityTypeEnum
{
    None,
    [FabricEntity(typeof(Workspace))]
    Workspace,
    [FabricEntity(typeof(DataPipeline))]
    DataPipeline,
    [FabricEntity(typeof(FabricEnvironment))]
    Environment,
    [FabricEntity(typeof(Notebook))]
    Notebook,
    // ... other entity types
}
```

### Adding a New Entity Type

To add a new entity type:

1. Add a new enum value to [`FabricEntityTypeEnum`](#fabricentitytypeenum)
2. Decorate it with the `[FabricEntity]` attribute specifying the corresponding Microsoft.Fabric.Api type
3. [Create a new orchestrator](#creating-an-orchestrator) class for the entity type
4. Add the new entity type to the health check validation ([see Testing section below](#required-health-check-validation))



### Creating an Orchestrator

Orchestrators inherit from `EntityOrchestratorBase<T>` where T is the entity type. Here's how to create a new one:

1. Create a new class in the Orchestrators folder
2. Inherit from `EntityOrchestratorBase<T>`
3. Implement required abstract members

Example:
```csharp
public class NewEntityOrchestrator : EntityOrchestratorBase<NewEntityType>
{
    public NewEntityOrchestrator(IConfiguration config, FabricClient fabricClient) 
        : base(config, fabricClient)
    {
    }

    protected override IEnumerable<NewEntityType> ListItems(Guid workspaceId)
    {
        // Implement entity-specific listing logic
        return FabricClient.NewEntities.ListAsync(workspaceId).Result;
    }
}
```

## Testing and Health Checks

### Required Health Check Validation

After adding a new entity type and its orchestrator, you MUST add it to the health check validation scenario to ensure it is properly validated against GitHub definitions. This is done in two places:

1. `Features/HealthCheck.feature`:
   - Add your new entity type to the "Fabric entities match their GitHub definitions" scenario
   - The entity will be automatically validated against its GitHub definition

2. `StepDefinitions/HealthCheckStepDefinitions.cs`:
   - The step definitions automatically handle validation for all entity types
   - No additional code is needed unless your entity requires special validation logic

Example health check scenario:
```gherkin
Scenario: Fabric entities match their GitHub definitions
    Given I am connected to the test Fabric workspace
    Then the following Fabric entities match their GitHub definitions:
        | EntityType    |
        | Workspace     |
        | DataPipeline  |
        | Notebook      |
        | NewEntity     |  # Add your new entity type here
```

## Using the Factory

The `FabricEntityOrchestratorFactory` provides several ways to get orchestrators:

```csharp
// Using the enum type
var orchestrator = factory.GetForEntity(FabricEntityTypeEnum.Workspace);

// Using generic type parameter
var workspaceOrchestrator = factory.GetForEntity<Workspace>();

// Direct type with specific orchestrator class
var typedOrchestrator = factory.Get<WorkspaceOrchestrator>();
```

## Best Practices

1. **Naming Convention**: Name orchestrators as `{EntityName}Orchestrator`
1. **Error Handling**: Always validate entity existence and permissions
1. **Documentation**: Document any specific behavior or requirements for new entity types

## Common Operations

Orchestrators typically provide operations like:

- Listing entities
- Creating new entities
- Updating existing entities
- Deleting entities
- Validating entity state

Implement these in your custom orchestrator as needed.

## Dependency Injection
Orchestrators are registered with the IoC container automatically

Ways to access the orchestrators:

### 1. Using an Orchestrator (`FabricEntityOrchestratorFactory`)
```csharp
public class MyStepDefinitions
{
    private readonly EntityOrchestratorFactory _factory;

    public MyStepDefinitions(EntityOrchestratorFactory factory)
    {
        _factory = factory;
    }

    [Given("something about a Workspace")]
    public void DoSomethingWithWorkspace()
    {
        var orchestrator = _factory.GetOrchestrator<Workspace>();
        var items = orchestrator.ListItems();
        // ...
    }
}
```

### 2. Using an Orchestrator (Dependency Injection)
```csharp
public class MyStepDefinitions
{
    private readonly WorkspaceOrchestrator _workspacceOrchestrator;

    public MyStepDefinitions(WorkspaceOrchestrator workspacceOrchestrator)
    {
        _fact_workspacceOrchestratorory = workspacceOrchestrator;
    }

    [Given("something about a Workspace")]
    public void DoSomethingWithWorkspace()
    {
        var items = _workspacceOrchestrator.ListItems();
        // ...
    }
}
```

## Error Scenarios

Common error scenarios to handle:

1. Entity not found
2. Permission denied
3. Invalid configuration
4. API failures

Use appropriate exception handling and logging in your orchestrator implementation.

## Testing New Orchestrators

1. Test error scenarios
1. Test integration with the Fabric API
1. Verify factory resolution works correctly

## Related Files

- `Entities/Enum/FabricEntityTypeEnum.cs` - Entity type definitions
- `Orchestrators/EntityOrchestratorBase.cs` - Base orchestrator class
- `Orchestrators/FabricEntityOrchestratorFactory.cs` - Orchestrator factory
- `Orchestrators/{EntityType}Orchestrator.cs` - Specific entity orchestrators
