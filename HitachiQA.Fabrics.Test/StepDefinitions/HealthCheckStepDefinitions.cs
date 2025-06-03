using HitachiQA.Fabrics.Test.Orchestrators;
using Microsoft.Fabric.Api.Notebook.Models;
using Microsoft.Fabric.Api.Utils;

namespace HitachiQA.Fabrics.Test.StepDefinitions;

[Binding]
public class HealthCheckStepDefinitions
{
    private readonly NoteBookOrchestrator _noteBookOrchestrator;

    private AsyncPageableResponse<Notebook> Notebooks { get; set; }

    public HealthCheckStepDefinitions(NoteBookOrchestrator noteBookOrchestrator)
    {
        _noteBookOrchestrator = noteBookOrchestrator;
    }

    [When(@"user gets Notebooks from fabrics")]
    public void WhenUserGetsNotebooksFromFabrics()
    {
        //get expected data out of github 


        // get current state in fabrics
        Notebooks = _noteBookOrchestrator.GetNotebooksAsync();


        //assert fabrics matches the expected in github
    }

    [Then(@"Notebooks should come back")]
    public async Task ThenNotebooksShouldComeBack()
    {
        await foreach (var notebook in Notebooks)
        {
            Console.WriteLine($"Notebook: {notebook.DisplayName}");
        }
    }
}
