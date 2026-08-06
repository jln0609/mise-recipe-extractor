using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.ValueObjects;
using Xunit;

namespace MiseRecipeExtractor.Core.Tests;

public class RecipeTests
{
    [Fact]
    public void AddVersion_ToFreshRecipe_AssignsVersionNumberOne()
    {
        var recipe = new Recipe
        {
            Source = new SourceMetadata() { Platform = "Xiaohongshu" }
        };

        var version = recipe.AddVersion(
            title: new LocalizedText { Original = "红烧肉", Translated = "Braised Pork" },
            ingredients: new List<Ingredient>(),
            steps: new List<Step>());

        Assert.Equal(1, version.VersionNumber);
    }

    [Fact]
    public void AddVersion_CalledTwice_IncrementsVersionNumber()
    {
        var recipe = new Recipe
        {
            Source = new SourceMetadata() { Platform = "Xiaohongshu" }
        };
        
        recipe.AddVersion(
            new LocalizedText { Original = "红烧肉" }, 
            new List<Ingredient>(), 
            new List<Step>());
        
        var secondVersion = recipe.AddVersion(
            new LocalizedText { Original = "红烧肉 v2" },
            new List<Ingredient>(),
            new List<Step>()
            );
        
        Assert.Equal(2, secondVersion.VersionNumber);
    }

    [Fact]
    public void CurrentVersion_ReturnsVersionWithHighestVersionNumber()
    {
        var recipe = new Recipe
        {
            Source = new SourceMetadata() { Platform = "Xiaohongshu" }
        };
        
        recipe.AddVersion(
            new LocalizedText { Original = "v1" }, 
            new List<Ingredient>(), 
            new List<Step>()
            );
        
        var latest = recipe.AddVersion(new LocalizedText { Original = "v2" }, new List<Ingredient>(), new List<Step>());
        
        var current = recipe.CurrentVersion;
        
        Assert.Same(latest, current);
    }
}
