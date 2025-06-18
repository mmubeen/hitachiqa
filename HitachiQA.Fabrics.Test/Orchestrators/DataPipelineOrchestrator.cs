using Fabrics.Test.Orchestrators;
using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.DataPipeline.Models;
using Microsoft.Fabric.Api.Warehouse.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class DataPipelineOrchestrator : EntityOrchestratorBase<DataPipeline>
{
    public DataPipelineOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    protected override IEnumerable<DataPipeline> ListItems(Guid workspaceId)
    {
        return FabricClient.DataPipeline.Items.ListDataPipelines(workspaceId);
    }


}
