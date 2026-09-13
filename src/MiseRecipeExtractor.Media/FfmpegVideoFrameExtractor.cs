using MiseRecipeExtractor.Core.Interfaces;

namespace MiseRecipeExtractor.Media;

public class FfmpegVideoFrameExtractor : IVideoFrameExtractor
{
    public Task<List<byte[]>> ExtractFramesAsync(byte[] video)
    {
        throw new NotImplementedException();
    }
}
