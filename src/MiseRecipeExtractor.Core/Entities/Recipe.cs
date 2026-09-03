using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Recipe
{
    public Guid Id { get;} = Guid.NewGuid();
    public SourceMetadata Source { get; init; } = null!;
    public List<RecipeVersion> Versions { get; init; } = new();

    public RecipeVersion CurrentVersion => Versions.MaxBy(v => v.VersionNumber)!;

    public RecipeVersion AddVersion(LocalizedText title, List<Ingredient> ingredients, List<Step> steps,
        RecipeStatus status = RecipeStatus.Draft, List<string>? warnings = null)
    {
        int nextVersionNumber = Versions.Count == 0 ? 1 : Versions.Max(v => v.VersionNumber) + 1;
        RecipeVersion version = new RecipeVersion
        {
            VersionNumber = nextVersionNumber,
            Title = title,
            Ingredients = ingredients,
            Steps = steps,
            Status = status,
            Warnings = warnings ?? new()
        };
        Versions.Add(version);
        return version;
    }

}