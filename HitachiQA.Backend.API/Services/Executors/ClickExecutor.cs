using HitachiQA.Backend.API.Interfaces;
using HitachiQA.Backend.API.Models;
using HitachiQA.Playwright;
using Microsoft.Playwright;

namespace HitachiQA.Backend.API.Services.Executors;

public class ClickExecutor : ExecutorBase
{
    public ClickExecutor(IPage page, TestAction action) : base(page, action)
    {

    }

    public override async Task ExecuteAsync()
    {
        ILocator? locator = await GetLocatorAsync(_action);
        await locator.Nth(_action.Nth).ClickAsync();
    }
}
