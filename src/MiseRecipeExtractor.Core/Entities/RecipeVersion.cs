using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class RecipeVersion
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public int VersionNumber { get; init; }
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;
    public LocalizedText Title { get; init; } = null!;
    public List<Ingredient> Ingredients { get; init; } = new();
    public List<Step> Steps { get; init; } = new();
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public enum RecipeStatus
{
    Draft,
    Tested,
    Adjusted
}