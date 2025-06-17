using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using FabricEnvironment = Microsoft.Fabric.Api.Environment.Models.Environment;
namespace HitachiQA.Fabrics.Test.Orchestrators;
public class EnvironmentOrchestrator
{
    private readonly FabricClient fabricClient;
    private readonly IConfiguration _config;
    public EnvironmentOrchestrator(FabricClient fabricClientParam, IConfiguration config)
    {
        fabricClient = fabricClientParam;
        _config = config;
    }
    public async Task<IEnumerable<FabricEnvironment>> GetEnvironmentsAsync()
    {
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID"));
        var res = fabricClient.Environment.Items.ListEnvironments(workspace);
        var result = new List<FabricEnvironment>();
        foreach (var env in res)
            result.Add(env);
        return await Task.FromResult(result);
    }
}

