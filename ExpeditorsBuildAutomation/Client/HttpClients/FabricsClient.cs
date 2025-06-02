using Newtonsoft.Json.Linq;
namespace ExpeditorsBuildAutomation.Client.HttpClients;

public class FabricsClient
{
    private readonly HttpClient httpClient;

    public FabricsClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }
    public async Task<JObject> ListNotebooks(string workspaceId) { 
        var request = new HttpRequestMessage();
        request.Method = HttpMethod.Get;
        request.RequestUri = new($"v1/workspaces/{workspaceId}/notebooks",UriKind.Relative);
        var response = await httpClient.SendAsync(request);
        var responseObject = await response.ParseIntoJObjectAsync();
        return responseObject;
    }
}
