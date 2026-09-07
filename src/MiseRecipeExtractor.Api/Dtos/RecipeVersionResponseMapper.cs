using MiseRecipeExtractor.Core.Entities;

namespace MiseRecipeExtractor.Api.Dtos;

public class RecipeVersionResponseMapper
{
    public static RecipeVersionResponse ToResponse(RecipeVersion version)
    {
        return new RecipeVersionResponse()
        {
            VersionNumber = version.VersionNumber,
            Status = version.Status.ToString(),
            TitleOriginal = version.Title.Original,
            TitleTranslated = version.Title.Translated,
            Ingredients = version.Ingredients.Select(i => new IngredientDto
            {
                NameOriginal = i.Name.Original,
                NameTranslated = i.Name.Translated,
                QuantityOriginalText = i.Quantity.OriginalText,
                QuantityAmount = i.Quantity.Amount,
                QuantityUnit = i.Quantity.Unit,
                QuantityConfidence = i.Quantity.Confidence.ToString(),
                Notes = i.Notes
            }).ToList(),
            Steps = version.Steps.Select(s => new StepDto
            {
                Order = s.Order,
                TextOriginal = s.Text.Original,
                TextTranslated = s.Text.Translated,
                DurationSeconds = s.DurationSeconds,
                OrderIsInferred = s.OrderIsInferred
            }).ToList(),
            Warnings = version.Warnings,
            Notes = version.Notes,
            CreatedAt = version.CreatedAt
        };
    }
}