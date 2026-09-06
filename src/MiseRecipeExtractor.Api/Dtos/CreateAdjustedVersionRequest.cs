namespace MiseRecipeExtractor.Api.Dtos;

public class CreateAdjustedVersionRequest
{
    public string TitleOriginal { get; set; } = "";
    public string? TitleTranslated { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = new();
    public List<StepDto> Steps { get; set; } = new();
}