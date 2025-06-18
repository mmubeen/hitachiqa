using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Warehouse.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class WarehouseOrchestrator : EntityOrchestratorBase<Warehouse>
{

    public WarehouseOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    protected override IEnumerable<Warehouse> ListItems(Guid workspaceId)
    {
        return FabricClient.Warehouse.Items.ListWarehouses(workspaceId);
    }
}
