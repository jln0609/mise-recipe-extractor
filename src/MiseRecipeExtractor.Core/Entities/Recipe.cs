namespace MiseRecipeExtractor.Core.Entities;

public class Recipe
{
    public Guid Id { get; set; }
    public SourceMetadata Source { get; set; } = null!;
    public List<RecipeVersion> Versions { get; set; } = new();

    public RecipeVersion CurrentVersion => Versions.OrderBy(v => v.CreatedAt).First();

}