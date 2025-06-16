using HitachiQA.Fabrics.Test.Orchestrators;
using Microsoft.Fabric.Api.DataPipeline.Models;
using Microsoft.Fabric.Api.Lakehouse.Models;
using Microsoft.Fabric.Api.Notebook.Models;
using FabricEnvironment = Microsoft.Fabric.Api.Environment.Models.Environment;

namespace HitachiQA.Fabrics.Test.StepDefinitions;

[Binding]
public class HealthCheckStepDefinitions
{
    private readonly NoteBookOrchestrator _noteBookOrchestrator;
    private readonly DataPipelineOrchestrator _dataPipelineOrchestrator;
    private readonly LakehouseOrchestrator _lakehouseOrchestrator;
    private readonly EnvironmentOrchestrator _environmentOrchestrator;

    private IEnumerable<Notebook> Notebooks { get; set; }
    private IEnumerable<DataPipeline> DataPipelines { get; set; }
    private IEnumerable<Lakehouse> Lakehouses { get; set; }
    private IEnumerable<FabricEnvironment> Environments { get; set; }


    public HealthCheckStepDefinitions(
        NoteBookOrchestrator noteBookOrchestrator,
        DataPipelineOrchestrator dataPipelineOrchestrator,
        LakehouseOrchestrator lakehouseOrchestrator,
        EnvironmentOrchestrator environmentOrchestrator)
    {
        _noteBookOrchestrator = noteBookOrchestrator;
        _dataPipelineOrchestrator = dataPipelineOrchestrator;
        _lakehouseOrchestrator = lakehouseOrchestrator;
        _environmentOrchestrator = environmentOrchestrator;
    }

    [When(@"user gets Notebooks from fabrics")]
    public async Task WhenUserGetsNotebooksFromFabrics()
    {
        //get expected data out of github 
        Console.WriteLine("huh1");


        // get current state in fabrics
        Notebooks = await _noteBookOrchestrator.GetNotebooksAsync();
        DataPipelines = await _dataPipelineOrchestrator.GetDataPipelinesAsync();
        Lakehouses = await _lakehouseOrchestrator.GetLakehousesAsync();
        Environments = await _environmentOrchestrator.GetEnvironmentsAsync();

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

        Log.Info($"Lakehouse count: {Lakehouses.Count()}");
        foreach (var item in Lakehouses)
            Log.Info($"Lakehouse name: {item.DisplayName}");

        Log.Info($"Environment count: {Environments.Count()}");
        foreach (var item in Environments)
            Log.Info($"Environment name: {item.DisplayName}");

    }
}
