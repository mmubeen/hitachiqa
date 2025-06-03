using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Notebook.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class NoteBookOrchestrator
{
    private readonly FabricClient fabricClient;
    private readonly IConfiguration _config;

    public NoteBookOrchestrator(FabricClient fabricCLient, IConfiguration config)
    {
        fabricClient = fabricCLient;
        _config = config;
    }

    public Microsoft.Fabric.Api.Utils.AsyncPageableResponse<Notebook> GetNotebooksAsync()
    {        
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID")); 
        var res = fabricClient.Notebook.Items.ListNotebooksAsync(workspace);
        return res;
    }

    public async Task<Notebook> GetNotebookByNameAsync(string displayName)
    {
        var notebooks = GetNotebooksAsync();
        await foreach (var notebook in notebooks) {
            if(notebook.DisplayName== displayName)
            {
                return notebook;
            }
        }
        return null;
    }
}
