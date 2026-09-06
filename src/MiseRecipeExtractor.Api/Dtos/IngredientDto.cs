namespace MiseRecipeExtractor.Api.Dtos;

public class IngredientDto
{
    public string NameOriginal { get; set; } = "";
    public string? NameTranslated { get; set; }
    public string QuantityOriginalText { get; set; } = "";
    public double? QuantityAmount { get; set; }
    public string? QuantityUnit { get; set; }
    public string QuantityConfidence { get; set; } = "Unspecified";
    public string? Notes { get; set; }
}