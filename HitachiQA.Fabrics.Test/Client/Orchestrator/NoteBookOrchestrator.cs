using HitachiQA.Fabrics.Test.Client.HttpClients;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace HitachiQA.Fabrics.Test.Client.Orchestrator;

public class NoteBookOrchestrator
{
    private readonly FabricsClient fabricsClient;
    private readonly IConfiguration _config;

    public NoteBookOrchestrator(FabricsClient fabricCLient, IConfiguration config)
    {
        fabricsClient = fabricCLient;
        _config = config;
    }

    public async Task<JArray> GetNotebooks()
    {        
        var workspace = _config.GetVariable("WORKSPACE_ID"); 
        var noteBooks = await fabricsClient.ListNotebooks(workspace);
        return (JArray)noteBooks["value"];
    }

    public async Task<JObject> GetNotebookByName(string displayName)
    {
        var notebooks = await GetNotebooks();
        foreach (var notebook in notebooks) {
            if(notebook["displayName"].ToString() == displayName)
            {
                return (JObject)notebook;
            }

        }
        return null;
    }
}
