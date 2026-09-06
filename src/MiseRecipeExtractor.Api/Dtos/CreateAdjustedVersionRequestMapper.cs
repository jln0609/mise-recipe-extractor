using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Api.Dtos;

public class CreateAdjustedVersionRequestMapper
{
    public static (LocalizedText Title, List<Ingredient> Ingredients, List<Step> Steps) ToDomain(
        CreateAdjustedVersionRequest request)
    {
        LocalizedText title = new LocalizedText()
        {
            Original = request.TitleOriginal,
            Translated = request.TitleTranslated
        };
        
        List<Ingredient> ingredients = request.Ingredients.Select(i => new Ingredient
        {
            Name = new LocalizedText() {Original = i.NameOriginal, Translated = i.NameTranslated},
            Quantity = new Quantity()
            {
                OriginalText = i.QuantityOriginalText,
                Amount = i.QuantityAmount,
                Unit = i.QuantityUnit,
                Confidence = Enum.Parse<ConfidenceLevel>(i.QuantityConfidence)
            },
            Notes = i.Notes
        }).ToList();
        
        List<Step> steps = request.Steps.Select(s => new Step
        {
            Order = s.Order,
            Text = new LocalizedText() { Original = s.TextOriginal, Translated = s.TextTranslated },
            DurationSeconds = s.DurationSeconds,
            OrderIsInferred = s.OrderIsInferred
        }).ToList();
        
        return (title, ingredients, steps);
    }
}