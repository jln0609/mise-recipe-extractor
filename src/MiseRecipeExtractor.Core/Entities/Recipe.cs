using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Recipe(SourceMetadata source)
{
    public Guid Id { get;} = Guid.NewGuid();
    public SourceMetadata Source { get; } = source;
    public List<RecipeVersion> Versions { get; set; } = new();

    public RecipeVersion CurrentVersion => Versions.OrderBy(v => v.CreatedAt).First();

    public RecipeVersion AddVersion(LocalizedText title, List<Ingredient> ingredients, List<Step> steps,
        RecipeStatus status = RecipeStatus.Draft)
    {
        int nextVersionNumber = Versions.Count == 0 ? 1 : Versions.Max(v => v.VersionNumber) + 1;
        var version = new RecipeVersion(nextVersionNumber, title, ingredients, steps, status);
        Versions.Add(version);
        return version;
    }

}