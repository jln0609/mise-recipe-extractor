namespace MiseRecipeExtractor.Core.Entities;

public class SourceMetadata(string platform, string? sourceUrl, string originalLanguage = "zh")
{
    public string Platform { get; } = platform;
    public string? SourceUrl { get; } = sourceUrl;
    public string OriginalLanguage { get; } = originalLanguage;
    public DateTime ExtractedAt { get; } = DateTime.UtcNow;
}