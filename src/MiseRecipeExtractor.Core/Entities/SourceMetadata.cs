namespace MiseRecipeExtractor.Core.Entities;

public class SourceMetadata
{
    public string Platform { get; set; } = "Xiaohongshu";
    public string? SourceUrl { get; set; }
    public string OriginalLanguage { get; set; } = "zh";
    public DateTime ExtractedAt { get; set; }
}