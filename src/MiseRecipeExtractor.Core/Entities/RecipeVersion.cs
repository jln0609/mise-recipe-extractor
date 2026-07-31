using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class RecipeVersion
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public RecipeStatus Status { get; set; }
    public LocalizedText Title { get; set; } = null!;
    public List<Ingredient> Ingredients { get; set; } = new();
    public List<Step> Steps { get; set; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum RecipeStatus
{
    Draft,
    Tested,
    Adjusted
}