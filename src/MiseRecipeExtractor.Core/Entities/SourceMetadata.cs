namespace MiseRecipeExtractor.Core.Entities;

public class SourceMetadata
{
    public string Platform { get; init; } = string.Empty;
    public string? SourceUrl { get; init; }
    public string OriginalLanguage { get; init; } = "zh";
    public DateTime ExtractedAt { get; init; } = DateTime.UtcNow;
}