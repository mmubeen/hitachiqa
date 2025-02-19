using HitachiQA.Backend.API.Interfaces;
using HitachiQA.Backend.API.Models;
using HitachiQA.Backend.API.Services.Executors;
using HitachiQA.Hooks.Browsers;
using HitachiQA.Playwright;
using Microsoft.Playwright;
using System.Reflection;

namespace HitachiQA.Backend.API.Services;

public class TestRunner : ITestRunner
{
    private readonly PlaywrightHook _playwrightHook;
    private readonly ILogger<TestRunner> _logger;

    public TestRunner(PlaywrightHook hook, ILogger<TestRunner> logger)
    {
        _playwrightHook = hook;
        _logger = logger;
    }

    public async Task<TestRun> RunAsync(TestRun run)
    {
        if (run.Driver != DriverEnum.Playwright)
            throw new NotImplementedException($"running with driver {run.Driver.ToString()} not implmented");


        PlaywrightHook.InstallPlaywright();

        foreach(var scenario in run.Scenarios)
        {
            await _playwrightHook.InvokeBrowserAsync(run.BrowserType.ToString(), run.Host);
            await _playwrightHook.PlaywrightPage.GotoAsync(run.Host);
            try
            {
                await RunScenario(scenario, _playwrightHook.PlaywrightPage);
                
            }
            finally
            {
                var video = _playwrightHook.PlaywrightPage.Video!;
                var videoPath = await video.PathAsync();
                scenario.VideoFilePath = Path.GetFullPath(videoPath);
                scenario.VideoFilename = Path.GetFileName(videoPath);
                await _playwrightHook.PlaywrightPage.CloseAsync();
            }
        }

        return run;
    }

    private async Task RunScenario(TestScenario scenario, IPage page)
    {
        try
        {
            foreach (var step in scenario.Steps)
            {
                await RunStep(step, page);
            }
        }
        catch(Exception ex)
        {
            scenario.Outcome = OutcomeEnum.Failed;
            _logger.LogError("Failed executing scenario {} - {}", scenario.Name, ex.Message);
            throw;
            
        }
        scenario.Outcome = OutcomeEnum.Passed;
    }

    private async Task RunStep(TestStep step, IPage page)
    {
        try { 
            foreach (var action in step.Actions)
            {
                await RunAction(action, page);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed executing step {} - {}", step.Description, ex.Message);
            throw;
        }
    }

    private async Task RunAction(TestAction action, IPage page)
    {
        try
        {
            var executorType = GetImplementationType(action.Type);
            var executor = (ExecutorBase)Activator.CreateInstance(executorType, page, action)!;
            await executor.ExecuteAsync();
        }
        catch (Exception)
        {
            _logger.LogError("Failed executing action {Action}", Log.stringify(action));
            throw;
        }
    }

    private static Type GetImplementationType(ActionTypeEnum actionType)
    {
        var type = actionType.GetType();
        var member = type.GetMember(actionType.ToString()).FirstOrDefault();

        var attribute = member?.GetCustomAttribute<ActionExecutorAttribute>();
        return attribute?.ImplementationType?? throw new NullReferenceException($"No implementation type found for {actionType}");
    }
}
