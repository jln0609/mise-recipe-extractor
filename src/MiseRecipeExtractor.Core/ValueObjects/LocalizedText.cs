namespace MiseRecipeExtractor.Core.ValueObjects;

public class LocalizedText(string original, string? translated = null)
{
    public string Original { get; } = original;
    public string? Translated { get; set; } = translated;
}