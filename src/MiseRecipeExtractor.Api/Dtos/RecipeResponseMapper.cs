using MiseRecipeExtractor.Core.Entities;

namespace MiseRecipeExtractor.Api.Dtos;

public static class RecipeResponseMapper
{
    public static RecipeResponse ToResponse(Recipe recipe)
    {
        RecipeVersion current = recipe.CurrentVersion;
        return new RecipeResponse()
        {
            Id = recipe.Id,
            Platform = recipe.Source.Platform,
            SourceUrl = recipe.Source.SourceUrl,
            CurrentVersionNumber = current.VersionNumber,
            TitleOriginal = current.Title.Original,
            TitleTranslated = current.Title.Translated,
            Status = current.Status.ToString(),
            Warnings = current.Warnings
        };
    }
}