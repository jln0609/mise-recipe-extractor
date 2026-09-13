using MiseRecipeExtractor.Core.Interfaces;

namespace MiseRecipeExtractor.Media;

public class FfmpegImagePreprocessor : IImagePreprocessor
{
    public Task<List<byte[]>> PrepareForExtractionAsync(List<byte[]> images)
    {
        throw new NotImplementedException();
    }
}
