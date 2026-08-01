using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;

namespace MiseRecipeExtractor.Api.Fakes;

public class InMemoryRecipeRepository: IRecipeRepository
{
    private readonly List<Recipe> _recipes = new();

    public Task<Recipe?> GetByIdAsync(Guid id)
    {
        var recipe = _recipes.FirstOrDefault(r => r.Id == id);
        return Task.FromResult(recipe);
    }

    public Task<List<Recipe>> GetAllAsync()
    {
        return Task.FromResult(_recipes);
    }

    public Task AddAsync(Recipe recipe)
    {
        _recipes.Add(recipe);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Recipe recipe)
    {
        var index = _recipes.FindIndex(r => r.Id == recipe.Id);
        if (index == -1)
        {
            throw new InvalidOperationException($"Recipe {recipe.Id} not found");
        }
        _recipes[index] = recipe;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        _recipes.RemoveAll(r => r.Id == id);
        return Task.CompletedTask;
    }
}