using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Core.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class WorkspaceOrchestrator : EntityOrchestratorBase<Workspace>
{

    public WorkspaceOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    public Guid GetConfiguredWorkspaceId => WorkspaceId;

    protected override IEnumerable<Workspace> ListItems(Guid workspaceId)
    {
        throw new InvalidOperationException("Listing workspaces is not supported provided a workspaceId");
    }

    public override IEnumerable<Workspace> ListItems()
    {
        return FabricClient.Core.Workspaces.ListWorkspaces().ToArray();
    }
}
