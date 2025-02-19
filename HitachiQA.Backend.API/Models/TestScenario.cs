namespace HitachiQA.Backend.API.Models;

public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public OutcomeEnum Outcome { get; set; } = OutcomeEnum.Failed;
    public List<TestStep> Steps { get; set; } = new();
    public string? VideoFilename { get; set; }
    public string? VideoFilePath { get; set; }
}
