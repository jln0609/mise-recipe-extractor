using MiseRecipeExtractor.Core.Entities;

namespace MiseRecipeExtractor.Core.Interfaces;

public interface IRecipeExtractor
{
    Task<ExtractionResult> ExtractAsync(List<byte[]> images);
}

public class ExtractionResult
{
    public RecipeVersion ExtractedVersion { get; set; } = null!;
    public List<string> Warnings { get; set; } = new();
    public string DetectedSourceLanguage { get; set; } = string.Empty;
}