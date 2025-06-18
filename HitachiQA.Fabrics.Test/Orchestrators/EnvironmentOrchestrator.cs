using Fabrics.Test.Orchestrators;
using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.DataPipeline.Models;
using FabricEnvironment = Microsoft.Fabric.Api.Environment.Models.Environment;
namespace HitachiQA.Fabrics.Test.Orchestrators;


public class EnvironmentOrchestrator : EntityOrchestratorBase<FabricEnvironment>
{
    public EnvironmentOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    protected override IEnumerable<FabricEnvironment> ListItems(Guid workspaceId)
    {
        return FabricClient.Environment.Items.ListEnvironments(workspaceId);
    }
}

