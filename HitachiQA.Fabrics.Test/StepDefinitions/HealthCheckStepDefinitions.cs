using HitachiQA.Fabrics.Test.Orchestrators;
using Microsoft.Fabric.Api.DataPipeline.Models;
using Microsoft.Fabric.Api.Notebook.Models;

namespace HitachiQA.Fabrics.Test.StepDefinitions;

[Binding]
public class HealthCheckStepDefinitions
{
    private readonly NoteBookOrchestrator _noteBookOrchestrator;
    private readonly DataPipelineOrchestrator _dataPipelineOrchestrator;

    private IEnumerable<Notebook> Notebooks { get; set; }
    private IEnumerable<DataPipeline> DataPipelines { get; set; }

    public HealthCheckStepDefinitions(
        NoteBookOrchestrator noteBookOrchestrator,
        DataPipelineOrchestrator dataPipelineOrchestrator)
    {
        _noteBookOrchestrator = noteBookOrchestrator;
        _dataPipelineOrchestrator = dataPipelineOrchestrator;
    }

    [When(@"user gets Notebooks from fabrics")]
    public async Task WhenUserGetsNotebooksFromFabrics()
    {
        //get expected data out of github 
        Console.WriteLine("huh1");


        // get current state in fabrics
        Notebooks = await _noteBookOrchestrator.GetNotebooksAsync();
        DataPipelines = await _dataPipelineOrchestrator.GetDataPipelinesAsync();

        //assert fabrics matches the expected in github
    }

    [Then(@"Notebooks should come back")]
    public void ThenNotebooksShouldComeBack()
    {
        Log.Info("Notebook count: " + Notebooks.Count());
        foreach(var item in Notebooks)
        {
            Log.Info("Notebook name: "+item.DisplayName);
        }

        Log.Info("DataPipeline count: " + DataPipelines.Count());
        foreach (var item in DataPipelines)
        {
            Log.Info("DataPipeline name: " + item.DisplayName);
        }
    }
}
