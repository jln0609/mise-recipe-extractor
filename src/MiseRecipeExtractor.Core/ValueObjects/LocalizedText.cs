namespace MiseRecipeExtractor.Core.ValueObjects;

public class LocalizedText
{
    public string Original { get; set; } = string.Empty;
    public string? Translated { get; set; }
}