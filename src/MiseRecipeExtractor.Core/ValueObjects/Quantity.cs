namespace MiseRecipeExtractor.Core.ValueObjects;

public class Quantity(string originalText, double? amount = null, string? unit = null, ConfidenceLevel confidence = ConfidenceLevel.Unspecified)
{
    public double? Amount { get; set; } = amount;
    public string? Unit {get; set; } = unit;
    public string OriginalText { get; } = originalText;
    public ConfidenceLevel Confidence { get; set; } = confidence;
}

public enum ConfidenceLevel
{
    Explicit,
    Estimated,
    Unspecified
}