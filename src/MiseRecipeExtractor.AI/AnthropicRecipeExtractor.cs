using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.VisualBasic.CompilerServices;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.AI;

public class AnthropicRecipeExtractor : IRecipeExtractor
{
    private readonly HttpClient _httpClient;
    private readonly string _systemPrompt;
    private readonly string _toolSchemaJson;

    public AnthropicRecipeExtractor(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var baseDirectory = AppContext.BaseDirectory;
        _systemPrompt = File.ReadAllText(Path.Combine(baseDirectory, "Prompts", "ExtractRecipeSystemPrompt.md"));
        _toolSchemaJson = File.ReadAllText(Path.Combine(baseDirectory, "Schemas", "ExtractRecipeTool.json"));
    }

    public async Task<ExtractionResult> ExtractAsync(List<byte[]> images)
    {
        JsonObject requestBody = BuildRequestBody(images);

        var response = await _httpClient.PostAsJsonAsync(("v1/messages"), requestBody);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadFromJsonAsync<JsonObject>()
                           ?? throw new InvalidOperationException(
                               "Anthropic response body was empty or not valid JSON.");
        
        JsonObject toolInput = ExtractToolUseInput(responseJson);

        var dto = toolInput.Deserialize<ExtractedRecipeDto>(DeserializeOptions)
                  ?? throw new InvalidOperationException(
                      "Failed to deserialize tool_use input into ExtractedRecipeDto.");

        return MapToExtractionResult(dto);
    }

    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private static ExtractionResult MapToExtractionResult(ExtractedRecipeDto dto)
    {
        RecipeVersion recipeVersion = new RecipeVersion()
        {
            VersionNumber = 1,
            Title = MapLocalizedText(dto.Title),
            Ingredients = dto.Ingredients.Select(MapIngredient).ToList(),
            Steps = dto.Steps.Select(MapStep).ToList(),
        };

        return new ExtractionResult()
        {
            ExtractedVersion = recipeVersion,
            Warnings = dto.Warnings,
            DetectedSourceLanguage = dto.DetectedSourceLanguage
        };
    }
    
    // request-building helpers

    private JsonObject BuildRequestBody(List<byte[]> images)
    {
        JsonArray contentBlocks = BuildImageContentBlocks(images);
        JsonNode toolSchema = JsonNode.Parse(_toolSchemaJson)!;

        return new JsonObject
        {
            ["model"] = "claude-sonnet-4-6",
            ["max_tokens"] = 4096,
            ["system"] = _systemPrompt,
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = contentBlocks
                }
            },
            ["tools"] = new JsonArray(toolSchema),
            ["tool_choices"] = new JsonObject
            {
                ["type"] = "tool",
                ["name"] = "extract_recipe"
            }
        };
    }

    private static JsonArray BuildImageContentBlocks(List<byte[]> images)
    {
        JsonArray contentBlocks = new JsonArray();

        foreach (byte[] image in images)
        {
            contentBlocks.Add(new JsonObject
            {
                ["type"] = "image",
                ["source"] = new JsonObject
                {
                    ["type"] = "base64",
                    ["media_type"] = DetectMediaType(image),
                    ["data"] = Convert.ToBase64String(image)
                }
            });
        }

        contentBlocks.Add(new JsonObject
        {
            ["type"] = "text",
            ["text"] =
                $"These {images.Count} images are all from the same recipe post. Extract the recipe using the extract_recipe tool."
        });

        return contentBlocks;
    }

    private static string DetectMediaType(byte[] imageBytes)
    {
        if (imageBytes.Length >= 8 &&
            imageBytes[0] == 0x89 && imageBytes[1] == 0x50 &&
            imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
        {
            return "image/png";
        }

        if (imageBytes.Length >= 3 &&
            imageBytes[0] == 0xFF && imageBytes[1] == 0xD8 &&
            imageBytes[2] == 0xFF)
        {
            return "image/jpeg";
        }

        throw new NotSupportedException(
            $"Unsupported image format {imageBytes.Length}. Only JPEG and PNG images are supported.");
    }

    // response-parsing helpers
    private static JsonObject ExtractToolUseInput(JsonObject responseJson)
    {
        JsonArray contentArray = responseJson["content"]?.AsArray()
                                 ?? throw new InvalidOperationException(
                                     "Anthropic Repone did not contain a 'content' array.");

        JsonObject toolUseBlock = contentArray.FirstOrDefault(block => block?["type"]?.GetValue<string>() == "tool_use")
                                      as JsonObject
                                  ?? throw new InvalidOperationException("Anthropic response did not contain a 'tool_use' object.");
        
        return toolUseBlock["input"]?.AsObject()
               ?? throw new InvalidOperationException("tool_use block did not contain an 'input' object.");
    }
    
    // DTO -> domain mapping helpers

    private static LocalizedText MapLocalizedText(LocalizedTextDto dto)
    {
        return new LocalizedText()
        {
            Original = dto.Original,
            Translated = dto.Translated,
        };
    }

    private static Quantity MapQuantity(QuantityDto dto)
    {
        if (!Enum.TryParse<ConfidenceLevel>(dto.Confidence, ignoreCase: true, out ConfidenceLevel confidence))
        {
            confidence = ConfidenceLevel.Unspecified;
        }

        return new Quantity()
        {
            OriginalText = dto.OriginalText,
            Amount = dto.Amount,
            Unit = dto.Unit,
            Confidence = confidence
        };
    }

    private static Ingredient MapIngredient(IngredientDto dto)
    {
        return new Ingredient()
        {
            Name = MapLocalizedText(dto.Name),
            Quantity = MapQuantity(dto.Quantity),
            Notes = dto.Notes
        };
    }

    private static Step MapStep(StepDto dto)
    {
        return new Step()
        {
            Order = dto.Order,
            Text = MapLocalizedText(dto.Text),
            DurationSeconds = dto.DurationSeconds,
            OrderIsInferred = dto.OrderIsInferred
        };
    }
    
    // DTOs
    private class ExtractedRecipeDto
    {
        public string DetectedSourceLanguage { get; set; } = string.Empty;
        public LocalizedTextDto Title { get; set; } = null!;
        public List<IngredientDto> Ingredients { get; set; } = new();
        public List<StepDto> Steps { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    private class LocalizedTextDto
    {
        public string Original { get; set; } = null!;
        public string? Translated { get; set; }
    }

    private class IngredientDto
    {
        public LocalizedTextDto Name { get; set; } = null!;
        public QuantityDto Quantity { get; set; } = null!;
        public string? Notes { get; set; }
    }
    
    private class QuantityDto
    {
        public string OriginalText { get; set; } = string.Empty;
        public double? Amount { get; set; }
        public string? Unit { get; set; }
        public string Confidence { get; set; } = string.Empty;
    }

    private class StepDto
    {
        public int Order { get; set; }
        public LocalizedTextDto Text { get; set; } = null!;
        public int? DurationSeconds { get; set; }
        public bool OrderIsInferred { get; set; }
    }
}