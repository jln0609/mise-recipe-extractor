namespace MiseRecipeExtractor.Core.ValueObjects;

public class LocalizedText
{
    public string Original { get; init; } = string.Empty;
    public string? Translated { get; set; }
}