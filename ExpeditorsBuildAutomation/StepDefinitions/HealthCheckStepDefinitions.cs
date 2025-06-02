using ExpeditorsBuildAutomation.Client.HttpClients;
using ExpeditorsBuildAutomation.Client.Orchestrator;
using HitachiQA;
using Newtonsoft.Json.Linq;

namespace ExpeditorsBuildAutomation.StepDefinitions;

[Binding]
public class HealthCheckStepDefinitions
{
    private readonly NoteBookOrchestrator _noteBookOrchestrator;

    private JArray Notebooks { get; set; }

    public HealthCheckStepDefinitions(NoteBookOrchestrator noteBookOrchestrator)
    {
        _noteBookOrchestrator = noteBookOrchestrator;
    }

    [When(@"user gets Notebooks from fabrics")]
    public async Task WhenUserGetsNotebooksFromFabrics()
    {
        Notebooks = await _noteBookOrchestrator.GetNotebooks();
    }

    [Then(@"Notebooks should come back")]
    public void ThenNotebooksShouldComeBack()
    {
        Log.Info(Notebooks[0]);
    }
}
