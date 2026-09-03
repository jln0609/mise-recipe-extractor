using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Api.IntegrationTests;

public class FakeRecipeExtractor : IRecipeExtractor
{
    public Task<ExtractionResult> ExtractAsync(List<byte[]> images)
    {
        var result = new ExtractionResult()
        {
            DetectedSourceLanguage = "zh",
            Warnings = new List<string> { "Fake warning for test purposes." },
            ExtractedVersion = new RecipeVersion()
            {
                Title = new LocalizedText()
                {
                    Original = "测试食谱",
                    Translated = "Test Recipe"
                },
                Ingredients = new List<Ingredient>()
                {
                    new Ingredient()
                    {
                        Name = new LocalizedText() { Original = "面粉", Translated = "Flour" },
                        Quantity = new Quantity()
                        {
                            OriginalText = "200g",
                            Amount = 200,
                            Unit = "g",
                            Confidence = ConfidenceLevel.Explicit
                        }
                    },
                    new Ingredient()
                    {
                        Name = new LocalizedText() { Original = "盐", Translated = "Salt" },
                        Quantity = new Quantity()
                        {
                            OriginalText = "适量",
                            Amount = null,
                            Unit = null,
                            Confidence = ConfidenceLevel.Unspecified
                        }
                    }
                },
                Steps = new List<Step>()
                {
                    new Step()
                    {
                        Order = 1,
                        Text = new LocalizedText() { Original = "混合所有原料", Translated = "Mix all ingredients" },
                        OrderIsInferred = false
                    },
                    new Step()
                    {
                        Order = 2,
                        Text = new LocalizedText() { Original = "烘烤", Translated = "Bake" },
                        DurationSeconds = 1200,
                        OrderIsInferred = true
                    }
                }
            }
        };

        return Task.FromResult(result);
    }
}