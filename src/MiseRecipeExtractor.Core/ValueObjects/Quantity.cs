namespace MiseRecipeExtractor.Core.ValueObjects;

public class Quantity
{
    public double? Amount { get; set; }
    public string? Unit {get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public ConfidenceLevel Confidence { get; set; }
}

public enum ConfidenceLevel
{
    Explicit,
    Estimated,
    Unspecified
}