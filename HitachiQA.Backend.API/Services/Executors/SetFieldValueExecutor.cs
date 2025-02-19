using HitachiQA.Backend.API.Models;
using HitachiQA.Playwright;
using Microsoft.Playwright;

namespace HitachiQA.Backend.API.Services.Executors;

public class SetFieldValueExecutor : ExecutorBase
{
    public SetFieldValueExecutor(IPage page, TestAction action) : base(page, action)
    {

    }

    public override async Task ExecuteAsync()
    {
        var field = await GetLocatorAsync(_action);
        await field.SetFieldValueAsync(_action.Value);
    }
}
