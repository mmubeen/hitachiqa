using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Lakehouse.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class LakehouseOrchestrator : EntityOrchestratorBase<Lakehouse>
{
    public LakehouseOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    protected override IEnumerable<Lakehouse> ListItems(Guid workspaceId)
    {
        return FabricClient.Lakehouse.Items.ListLakehouses(workspaceId);
    }
}
