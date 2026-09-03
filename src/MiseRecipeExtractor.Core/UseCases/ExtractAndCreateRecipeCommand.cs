using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;

namespace MiseRecipeExtractor.Core.UseCases;

public class ExtractAndCreateRecipeCommand(IRecipeExtractor extractor, IRecipeRepository repository)
{
    public async Task<Recipe> ExecuteAsync(List<byte[]> images, string platform, string? sourceUrl)
    {
        ExtractionResult extraction = await extractor.ExtractAsync(images);

        Recipe recipe = new Recipe()
        {
            Source = new SourceMetadata()
            {
                Platform = platform,
                SourceUrl = sourceUrl,
                OriginalLanguage = extraction.DetectedSourceLanguage
            }
        };

        recipe.AddVersion(
            title: extraction.ExtractedVersion.Title,
            ingredients: extraction.ExtractedVersion.Ingredients,
            steps: extraction.ExtractedVersion.Steps,
            warnings: extraction.Warnings);
        
        await repository.AddAsync(recipe);
        
        return recipe;
    }
}