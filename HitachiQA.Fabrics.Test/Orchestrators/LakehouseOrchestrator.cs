using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Lakehouse.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class LakehouseOrchestrator
{
    private readonly FabricClient fabricClient;
    private readonly IConfiguration _config;

    public LakehouseOrchestrator(FabricClient fabricClient, IConfiguration config)
    {
        this.fabricClient = fabricClient;
        _config = config;
    }

    public async Task<IEnumerable<Lakehouse>> GetLakehousesAsync()
    {
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID"));
        var res = fabricClient.Lakehouse.Items.ListLakehouses(workspace);
        var result = new List<Lakehouse>();
        foreach (var lakehouse in res)
        {
            result.Add(lakehouse);
        }
        return await Task.FromResult(result);
    }
}
