namespace HitachiQA.Backend.API.Models;

public class TestAction
{
    public ActionTypeEnum Type { get; set; }
    public string Selector { get; set; } = string.Empty;
    public string XPath { get; set; } = string.Empty;
    public string CSSSelector { get; set; } = string.Empty;
    public int Nth { get; set; } = 0;
    public string? Value { get; set; } 
}
