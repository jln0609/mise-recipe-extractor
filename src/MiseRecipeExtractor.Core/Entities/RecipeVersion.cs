using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class RecipeVersion(int versionNumber, LocalizedText title,
    List<Ingredient> ingredients, List<Step> steps, RecipeStatus status = RecipeStatus.Draft)
{
    public Guid Id { get; } = Guid.NewGuid();
    public int VersionNumber { get; } = versionNumber;
    public RecipeStatus Status { get; set; } = status;
    public LocalizedText Title { get; } = title;
    public List<Ingredient> Ingredients { get;} = ingredients;
    public List<Step> Steps { get; } = steps;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; } = DateTime.Now;
}

public enum RecipeStatus
{
    Draft,
    Tested,
    Adjusted
}