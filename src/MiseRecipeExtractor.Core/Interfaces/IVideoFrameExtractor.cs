namespace MiseRecipeExtractor.Core.Interfaces;

public interface IVideoFrameExtractor
{
    Task<List<byte[]>> ExtractFramesAsync(byte[] video);
}
