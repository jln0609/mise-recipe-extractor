namespace MiseRecipeExtractor.Api.Dtos;

public class StepDto
{
    public int Order { get; set; }
    public string TextOriginal { get; set; } = "";
    public string? TextTranslated { get; set; }
    public int? DurationSeconds { get; set; }
    public bool OrderIsInferred { get; set; }
}