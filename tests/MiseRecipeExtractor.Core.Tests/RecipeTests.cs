using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.ValueObjects;
using Xunit;

namespace MiseRecipeExtractor.Core.Tests;

public class RecipeTests
{
    [Fact]
    public void AddVersion_ToFreshRecipe_AssignsVersionNumberOne()
    {
        var recipe = new Recipe(new SourceMetadata("Xiaohongshu", null));

        var version = recipe.AddVersion(title: new LocalizedText("红烧肉", "Braised Pork"),
            ingredients: new List<Ingredient>(), steps: new List<Step>());

        Assert.Equal(1, version.VersionNumber);
    }

    [Fact]
    public void AddVersion_CalledTwice_IncrementsVersionNumber()
    {
        var recipe = new Recipe(new SourceMetadata("Xiaohongshu", null));
        recipe.AddVersion(new LocalizedText("红烧肉"), new List<Ingredient>(), new List<Step>());
        
        var secondVersion = recipe.AddVersion(new LocalizedText("红烧肉 v2"), new List<Ingredient>(), new List<Step>());
        
        Assert.Equal(2, secondVersion.VersionNumber);
    }

    [Fact]
    public void CurrentVersion_ReturnsVersionWithHighestVersionNumber()
    {
        var recipe = new Recipe(new SourceMetadata("Xiaohongshu", null));
        recipe.AddVersion(new LocalizedText("v1"), new List<Ingredient>(), new List<Step>());
        var latest = recipe.AddVersion(new LocalizedText("v2"), new List<Ingredient>(), new List<Step>());
        
        var current = recipe.CurrentVersion;
        
        Assert.Same(latest, current);
    }
}
