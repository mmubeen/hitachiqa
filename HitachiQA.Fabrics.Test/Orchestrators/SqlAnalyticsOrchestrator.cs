using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.SqlAnalytics.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class SqlAnalyticsOrchestrator++++++++++






































































































{
    private readonly FabricClient fabricClient;
    private readonly IConfiguration _config;

    public SqlAnalyticsOrchestrator(FabricClient fabricClientParam, IConfiguration config)
    {
        fabricClient = fabricClientParam;
        _config = config;
    }

    public async Task<IEnumerable<SqlAnalyticsEndpoint>> GetSqlAnalyticsEndpointsAsync()
    {
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID"));
        var res = fabricClient.SqlAnalytics.Items.ListSqlAnalyticsEndpoints(workspace);
        var result = new List<SqlAnalyticsEndpoint>();

        foreach (var endpoint in res)
        {
            result.Add(endpoint);
        }

        return await Task.FromResult(result);
    }
}
