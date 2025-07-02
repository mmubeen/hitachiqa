using Fabrics.Test.Entities;
using Fabrics.Test.Entities.Enum;
using Fabrics.Test.Orchestrators.ExpectedProviders;
using HitachiQA.Fabrics.Test.Orchestrators;
using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api.Core.Models;
using System.Text;

namespace HitachiQA.Fabrics.Test.StepDefinitions;

[Binding]
public class HealthCheckStepDefinitions
{
    private readonly IExpectedProvider _expectedProvider;
    private readonly FabricEntityOrchestratorFactory _fabricEntityOrchestratorFactory;
    private readonly IConfiguration _config;
    private EntityInfo[] expectedEntities;
    private dynamic[] actualEntities;

    public HealthCheckStepDefinitions(
        IExpectedProvider expectedProvider,
        FabricEntityOrchestratorFactory fabricEntityOrchestratorFactory,
        IConfiguration config)
    {

        _expectedProvider = expectedProvider;
        _fabricEntityOrchestratorFactory = fabricEntityOrchestratorFactory;
        _config = config;
    }

    private void LogCollectionDetails<T>(IEnumerable<T> collection) where T : class
    {
        var collectionName = typeof(T).Name;
        Log.Info("");
        Log.Info($"{collectionName} count: {collection.Count()}");
        foreach (var item in collection)
        {
            var displayName = (item as dynamic)?.DisplayName ?? "Unknown";
            Log.Info($"{collectionName} name: {displayName}");
        }
    }

    [Given("the configured Fabric workspace is available")]
    public void GivenTheConfiguredFabricWorkspaceIsAvailable()
    {
        var orchestrator = _fabricEntityOrchestratorFactory.GetForEntity<Workspace>();
        var workspaces = orchestrator.ListItems();
        var workspace = workspaces.FirstOrDefault(w => w.Id==orchestrator.WorkspaceId);
        //Assert.IsNotNull(workspace, $"didn't find workspace (id: {workspace.Id})");
        Log.Info($"Loaded workspace {workspace.DisplayName} (id: {workspace.Id})");
    }

    [Given("GitHub repository contains entity definitions")]
    public void GivenGitHubRepositoryContainsEntityDefinitions()
    {
        //Assert.IsNotEmpty(_expectedProvider.EntitiyInfos);
    }

    [Given("the expected {string} definitions are loaded from GitHub")]
    public void GivenTheExpectedDefinitionsAreLoadedFromGitHub(FabricEntityTypeEnum type)
    {
        var sourceName = "Github repo";
        expectedEntities = _expectedProvider.EntitiyInfos.Where(info => info.Type == type.ToString()).ToArray();
        if(expectedEntities.Length == 0)
        {
            Log.Info($"Didn't find any entities of type {type} in the source: {sourceName}");
        }
        Log.Info($"Loaded {expectedEntities.Length} {type} from {sourceName}");
    }

    [When("the actual {string} entities are retrieved from the Fabric workspace")]
    public void WhenTheActualEntitiesAreRetrievedFromTheFabricWorkspace(FabricEntityTypeEnum type)
    {
        var sourceName = "Fabric Workspace";
        var orchestrator = _fabricEntityOrchestratorFactory.GetForEntity(type);
        var workspaces = orchestrator.ListItems();
        actualEntities = workspaces.Cast<dynamic>().ToArray();
        Log.Info($"Loaded {expectedEntities.Length} {type} from {sourceName}");
    }

    [Then("the actual entities should match the expected definitions")]
    public void ThenTheActualEntitiesShouldMatchTheExpectedDefinitions()
    {
        var actualNames = actualEntities.Select(x => (string)x.DisplayName).ToHashSet(); // fast lookup

        var missing = expectedEntities
            .Select(e => (string)e.DisplayName)
            .Where(name => !actualNames.Contains(name))
            .ToList();

        var desc = $"\n" +
                   $"Expected Entities: {string.Join(", ", expectedEntities.Select(e => (string)e.DisplayName))}\n" +
                   $"  Actual Entities: {string.Join(", ", actualNames)}";
        Log.Info(desc);
        if (missing.Any())
        {
            var message = $"Missing DisplayNames: {string.Join(", ", missing)}\n";
                          
            //Assert.Fail(message + desc);
        }
    }

    [Then("all required properties should be present")]
    public void ThenAllRequiredPropertiesShouldBePresent()
    {
        //tbd
    }

    [Then("no unexpected entities should exist in the workspace")]
    public void ThenNoUnexpectedEntitiesShouldExistInTheWorkspace()
    {
        var expectedCount = expectedEntities.Count();
        var actualCount = actualEntities.Count();
        if (expectedCount != actualCount)
        {
            var missing = expectedEntities
                .Where(e => !actualEntities.Any(a => a.DisplayName == e.DisplayName))
                .Select(e => e.DisplayName)
                .ToList();

            var extra = actualEntities
                .Where(a => !expectedEntities.Any(e => e.DisplayName == a.DisplayName))
                .Select(a => a.DisplayName)
                .ToList();

            var message = new StringBuilder();
            message.AppendLine($"Count mismatch: expected {expectedCount}, actual {actualCount}");

            if (missing.Any())
                message.AppendLine($"Missing: {string.Join(", ", missing)}");

            if (extra.Any())
                message.AppendLine($"Extra: {string.Join(", ", extra)}");

            //Assert.Fail(message.ToString());
        }
    }

}
