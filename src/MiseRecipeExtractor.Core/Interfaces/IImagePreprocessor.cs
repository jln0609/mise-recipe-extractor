namespace MiseRecipeExtractor.Core.Interfaces;

public interface IImagePreprocessor
{
    Task<List<byte[]>> PrepareForExtractionAsync(List<byte[]> images);
}
