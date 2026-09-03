using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Infrastructure.Tests;

public class EfRecipeRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly List<RecipeDbContext> _contexts = new();
    
    public EfRecipeRepositoryTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        
        using RecipeDbContext initContext = CreateContext();
        initContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        foreach (RecipeDbContext context in _contexts)
            context.Dispose();
        
        _connection.Dispose();
    }

    private RecipeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RecipeDbContext>().UseSqlite(_connection).Options;
        
        var context = new RecipeDbContext(options);
        _contexts.Add(context);
        return context;
    }

    private (RecipeDbContext, EfRecipeRepository) CreateScope()
    {
        RecipeDbContext context = CreateContext();
        return (context, new EfRecipeRepository(context));
    }

    [Fact]
    public async Task AddAsync_TheGetByIDAsync_ReturnsMatchingRecipe()
    {
        // arrange
        var (_, repository) = CreateScope();
        
        var recipe = new Recipe
        {
            Source = new SourceMetadata { Platform = "Xiaohongshu", SourceUrl = "https://example.com/post/789" }
        };
        recipe.AddVersion(
            title: new LocalizedText { Original = "红烧肉", Translated = "Braised Pork" },
            ingredients: new List<Ingredient>
            {
                new Ingredient
                {
                    Name = new LocalizedText { Original = "猪肉", Translated = "Pork" },
                    Quantity = new Quantity
                        { OriginalText = "500g", Amount = 500, Unit = "g", Confidence = ConfidenceLevel.Explicit }
                }
            },
            steps: new List<Step>
            {
                new Step { Order = 1, Text = new LocalizedText { Original = "切块", Translated = "Cut into pieces" } }
            },
            warnings: new List<string>() { "Test warning" });
        
        // act
        await repository.AddAsync(recipe);
        Recipe? retrieved = await repository.GetByIdAsync(recipe.Id);
        
        // assert
        Assert.NotNull(retrieved);
        Assert.Equal(recipe.Id, retrieved.Id);
        Assert.Equal("Xiaohongshu", retrieved.Source.Platform);
        Assert.Single(retrieved.Versions);
        Assert.Equal("红烧肉", retrieved.CurrentVersion.Title.Original);
        Assert.Single(retrieved.CurrentVersion.Ingredients);
        Assert.Single(retrieved.CurrentVersion.Steps);
        Assert.Single(retrieved.CurrentVersion.Warnings);
    }

    [Fact]
    public async Task UpdateAsync_ChangesFromDifferentContext_PersistsCorrectly()
    {
        // arrange
        var (_, writeRepo) = CreateScope();
        var (_, changesRepo) = CreateScope();
        var (_, updateRepo) = CreateScope();
        var (_, verifyRepo) = CreateScope();
            
        var recipe = new Recipe { Source = new SourceMetadata { Platform = "Xiaohongshu" } };
        recipe.AddVersion(
            title: new LocalizedText { Original = "红烧肉" },
            ingredients: new List<Ingredient>(),
            steps: new List<Step>());
        await writeRepo.AddAsync(recipe);
        
        // act
        
        Recipe? loadedForChanges = await changesRepo.GetByIdAsync(recipe.Id);
        Assert.NotNull(loadedForChanges);
        loadedForChanges.CurrentVersion.Status = RecipeStatus.Tested;
        loadedForChanges.CurrentVersion.Notes = "Tested and adjusted seasoning.";
        
        await updateRepo.UpdateAsync(loadedForChanges);
        
        Recipe? verified = await verifyRepo.GetByIdAsync(recipe.Id);
        
        Assert.NotNull(verified);
        Assert.Equal(RecipeStatus.Tested, verified.CurrentVersion.Status);
        Assert.Equal("Tested and adjusted seasoning.", verified.CurrentVersion.Notes);
    }
}