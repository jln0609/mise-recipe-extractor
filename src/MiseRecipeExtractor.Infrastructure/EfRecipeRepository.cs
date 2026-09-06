using Microsoft.EntityFrameworkCore;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;

namespace MiseRecipeExtractor.Infrastructure;

public class EfRecipeRepository(RecipeDbContext dbContext) : IRecipeRepository
{
    public async Task<Recipe?> GetByIdAsync(Guid id)
    {
        return await RecipesWithFullGraph().FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Recipe>> GetAllAsync()
    {
        return await RecipesWithFullGraph().ToListAsync();
    }

    public async Task AddAsync(Recipe recipe)
    {
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        Recipe? existing = await RecipesWithFullGraph().FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (existing == null)
        {
            throw new InvalidOperationException($"Recipe with id {recipe.Id} not found");
        }

        dbContext.Entry(existing).CurrentValues.SetValues(recipe);

        foreach (RecipeVersion version in recipe.Versions)
        {
            bool alreadyPersisted = await dbContext.Set<RecipeVersion>().AnyAsync(v => v.Id == version.Id);

            if (alreadyPersisted)
            {
                var trackedVersion = existing.Versions.First(v => v.Id == version.Id);
                dbContext.Entry(trackedVersion).CurrentValues.SetValues(version);
            }
            else
            {
                dbContext.Add(version);
                dbContext.Entry(version).State = EntityState.Added;
                foreach (Ingredient ingredient in version.Ingredients)
                {
                    dbContext.Entry(ingredient).State = EntityState.Added;
                }
                foreach (Step step in version.Steps)
                {
                    dbContext.Entry(step).State = EntityState.Added;
                }
            }
        }
        
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        Recipe? recipe = await dbContext.Recipes.FindAsync(id);
        if (recipe is not null)
        {
            dbContext.Recipes.Remove(recipe);
            await dbContext.SaveChangesAsync();
        }
    }

    private IQueryable<Recipe> RecipesWithFullGraph()
    {
        return dbContext.Recipes
            .Include(r => r.Versions)
            .ThenInclude(v => v.Ingredients)
            .Include(r => r.Versions)
            .ThenInclude(v => v.Steps);
    }
}