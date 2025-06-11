using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Notebook.Models;
using System.Collections;

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

    public async Task<IEnumerable<Notebook>> GetNotebooksAsync()
    {        
        var workspace = Guid.Parse(_config.GetVariable("WORKSPACE_ID"));
        var res = fabricClient.Notebook.Items.ListNotebooks(workspace);
        var result = new List<Notebook>();
        foreach (var notebook in res)
        {
            result.Add(notebook);
        }
        return await Task.FromResult(result);
    }

    public async Task<Notebook> GetNotebookByNameAsync(string displayName)
    {
        var notebooks = await GetNotebooksAsync();
        foreach (var notebook in notebooks) {
            if(notebook.DisplayName== displayName)
            {
                return notebook;
            }
        }
        return null;
    }
}
