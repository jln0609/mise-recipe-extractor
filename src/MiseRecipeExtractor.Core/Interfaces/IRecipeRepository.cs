using MiseRecipeExtractor.Core.Entities;

namespace MiseRecipeExtractor.Core.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(Guid id);
    Task<List<Recipe>> GetAllAsync();
    Task AddAsync(Recipe recipe);
    Task UpdateAsync(Recipe recipe);
    Task DeleteAsync(Guid id);
}