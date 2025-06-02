using HitachiQA.Fabrics.Test.Client.Orchestrator;
using Newtonsoft.Json.Linq;

namespace HitachiQA.Fabrics.Test.StepDefinitions;

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
        Log.Info(Notebooks);
    }
}
