using HitachiQA.Backend.API.Models;
using Microsoft.Playwright;

namespace HitachiQA.Backend.API.Services.Executors;

public class NavigateExecutor : ExecutorBase
{

    public NavigateExecutor(IPage page, TestAction action) : base(page, action)
    {
    }


    public override async Task ExecuteAsync()
    {
        if (_action.Value == null)
            throw new NullReferenceException($"{_action.Type} has null value, please provide url to navigate to");
        await _page.GotoAsync(_action.Value);
    }
}
