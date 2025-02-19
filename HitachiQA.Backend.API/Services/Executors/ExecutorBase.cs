using HitachiQA.Backend.API.Interfaces;
using HitachiQA.Backend.API.Models;
using HitachiQA.Playwright;
using Microsoft.Playwright;
using OpenQA.Selenium.BiDi.Modules.BrowsingContext;

namespace HitachiQA.Backend.API.Services.Executors;

public abstract class ExecutorBase: IActionExecutor
{
    protected readonly IPage _page;
    protected readonly TestAction _action;

    protected ExecutorBase(IPage page, TestAction action)
    {
        _page = page;
        _action = action;
    }

    public abstract Task ExecuteAsync();

    public async Task<ILocator> GetLocatorAsync(TestAction action)
    {
        ILocator? locator = null;
        if (!string.IsNullOrEmpty(_action.XPath))
        {
            locator = _page.Locator($"xpath={_action.XPath}");
        }
        else if (!string.IsNullOrEmpty(_action.CSSSelector))
        {
            locator = _page.Locator($"css={_action.CSSSelector}");
        }
        else if (!string.IsNullOrEmpty(_action.Selector))
        {
            locator = await _page.GetFieldAsync(_action.Selector);
        }

        if (locator == null)
        {
            throw new InvalidOperationException("Element not found using XPath, CSS Selector, or generic selector.");
        }
        return locator;
    }
    
}
