namespace HitachiQA.Backend.API.Models;

public class TestStep
{
    public string Description { get; set; } = string.Empty;
    public List<TestAction> Actions { get; set; } = [];
}
