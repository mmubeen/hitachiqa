using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.DataPipeline.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class DataPipelineOrchestrator
{
    private readonly FabricClient fabricClient;
    private readonly IConfiguration _config;

    public DataPipelineOrchestrator(FabricClient fabricCLient, IConfiguration config)
    {
        fabricClient = fabricCLient;
        _config = config;
    }

    public async Task<IEnumerable<DataPipeline>> GetDataPipelinesAsync()
    {        
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID"));
        var res = fabricClient.DataPipeline.Items.ListDataPipelines(workspace);
        var result = new List<DataPipeline>();
        foreach (var notebook in res)
        {
            result.Add(notebook);
        }
        return await Task.FromResult(result);
    }


}
