namespace MiseRecipeExtractor.Api.Dtos;

public class CreateRecipeRequest
{
    public string Platform { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string TitleOriginal { get; set; } = "";
    public string? TitleTranslated { get; set; }
}
