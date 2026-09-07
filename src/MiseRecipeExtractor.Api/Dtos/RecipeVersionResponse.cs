namespace MiseRecipeExtractor.Api.Dtos;

public class RecipeVersionResponse
{
    public int VersionNumber { get; set; }
    public string Status { get; set; } = "";
    public string TitleOriginal { get; set; } = "";
    public string? TitleTranslated { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<StepDto> Steps { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}