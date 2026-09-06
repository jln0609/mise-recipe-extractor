using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.UseCases;

public class CreateAdjustedVersionCommand(IRecipeRepository repository)
{
    public async Task<Recipe?> ExecuteAsync(Guid recipeId, LocalizedText title, List<Ingredient> ingredients,
        List<Step> steps)
    {
        Recipe? recipe = await repository.GetByIdAsync(recipeId);
        if (recipe == null)
        {
            return null;
        }
        
        recipe.AddVersion(title, ingredients, steps, status: RecipeStatus.Adjusted);
        
        await repository.UpdateAsync(recipe);
        
        return recipe;
    }
}