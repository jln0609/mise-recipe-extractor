namespace MiseRecipeExtractor.Api.Dtos;

public class RecipeResponse
{
    public Guid Id { get; set; }
    public string Platform { get; set; } = "";
    public string? SourceUrl { get; set; }
    public int CurrentVersionNumber { get; set; }
    public string TitleOriginal { get; set; } = "";
    public string? TitleTranslated { get; set; }
    public string Status { get; set; } = "";
}
