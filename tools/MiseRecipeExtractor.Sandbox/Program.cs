

using Microsoft.Extensions.Configuration;
using MiseRecipeExtractor.AI;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddUserSecrets("b3031a11-8477-484f-9cd1-9548227c8469")
    .Build();
    
string apiKey = configuration["Anthropic:ApiKey"]
    ?? throw new InvalidOperationException("Anthropic:ApiKey not found in User Secrets.");
    
HttpClient httpClient = new HttpClient()
{
    BaseAddress = new Uri("https://api.anthropic.com/")
};
httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

AnthropicRecipeExtractor extractor = new AnthropicRecipeExtractor(httpClient);

string screenshotsFolder = @"C:\Users\jli08\Documents\Git\MiseRecipeExtractor\test screenshots";
string[] imageFiles = Directory.GetFiles(screenshotsFolder,"*.png");
Console.WriteLine($"Loaded {imageFiles.Length} images from {screenshotsFolder}");

List<byte[]> images = imageFiles.Select(File.ReadAllBytes).ToList();

ExtractionResult result = await extractor.ExtractAsync(images);

Console.WriteLine($"Detected language: {result.DetectedSourceLanguage}");
Console.WriteLine($"Title: {result.ExtractedVersion.Title.Original} / {result.ExtractedVersion.Title.Translated}");
Console.WriteLine($"\nIngredients ({result.ExtractedVersion.Ingredients.Count}):");
foreach (var ingredient in result.ExtractedVersion.Ingredients)
{
    string name = ingredient.Name.Translated is not null
        ? $"{ingredient.Name.Translated} ({ingredient.Name.Original})"
        : ingredient.Name.Original;
    string amountDisply = ingredient.Quantity.Amount is not null
        ? $"{ingredient.Quantity.Amount}{ingredient.Quantity.Unit}"
        : ingredient.Quantity.OriginalText;
    
    Console.WriteLine($" - {name}: {amountDisply} [{ingredient.Quantity.Confidence}]" +
        (ingredient.Notes is not null ? $" ({ingredient.Notes})" : ""));
}

Console.WriteLine($"\nSteps ({result.ExtractedVersion.Steps.Count}):");
foreach (var step in result.ExtractedVersion.Steps.OrderBy(s => s.Order))
{
    string text = step.Text.Translated ?? step.Text.Original;
    string duration = step.DurationSeconds is not null ? $" [{step.DurationSeconds}]" : "";
    Console.WriteLine($" {step.Order}. {text}{duration}" + (step.OrderIsInferred ? " (order inferred)" : ""));
}

Console.WriteLine($"\nWarnings: {(result.Warnings.Count == 0 ? "none" : string.Join("; ", result.Warnings))}");