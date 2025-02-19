using Microsoft.Playwright;

namespace HitachiQA.Backend.API.Models;

public class TestRun
{
    public DriverEnum Driver { get; set; } = DriverEnum.Playwright;
    public BrowserTypeEnum BrowserType { get; set; } = BrowserTypeEnum.Chrome;
    public string Host { get; set; } = string.Empty;
    public List<TestScenario> Scenarios { get; set; } = new();
}
