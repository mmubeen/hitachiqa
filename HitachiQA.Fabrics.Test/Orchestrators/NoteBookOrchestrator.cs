using Microsoft.Extensions.Configuration;
using Microsoft.Fabric.Api;
using Microsoft.Fabric.Api.Lakehouse.Models;
using Microsoft.Fabric.Api.Notebook.Models;

namespace HitachiQA.Fabrics.Test.Orchestrators;

public class NoteBookOrchestrator : EntityOrchestratorBase<Notebook>
{
    public NoteBookOrchestrator(FabricClient fabricClient, IConfiguration config) : base(config, fabricClient)
    {

    }

    protected override IEnumerable<Notebook> ListItems(Guid workspaceId)
    {
        return FabricClient.Notebook.Items.ListNotebooks(workspaceId);
    }

    public Notebook GetNotebookByNameAsync(string displayName)
    {
        var notebooks = ListItems();
        foreach (var notebook in notebooks) {
            if(notebook.DisplayName == displayName)
            {
                return notebook;
            }
        }
        return null;
    }
}
