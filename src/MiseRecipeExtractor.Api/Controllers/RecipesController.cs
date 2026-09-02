using Microsoft.AspNetCore.Mvc;
using MiseRecipeExtractor.Api.Dtos;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController(IRecipeRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<RecipeResponse>>> GetAll()
    {
        List<Recipe> recipes = await repository.GetAllAsync();
        List<RecipeResponse> responses = recipes.Select(RecipeResponseMapper.ToResponse).ToList();
        return Ok(responses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeResponse>> GetById(Guid id)
    {
        var recipe = await repository.GetByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }
        return Ok(RecipeResponseMapper.ToResponse(recipe));
    }

    [HttpPost]
    public async Task<ActionResult<RecipeResponse>> Create(CreateRecipeRequest request)
    {
        Recipe recipe = new Recipe
        {
            Source = new SourceMetadata {Platform = request.Platform, SourceUrl = request.SourceUrl },
        };

        recipe.AddVersion(
            title: new LocalizedText { Original = request.TitleOriginal, Translated = request.TitleTranslated },
            ingredients: new List<Ingredient>(),
            steps: new List<Step>()
        );
        
        await repository.AddAsync(recipe);
        
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, RecipeResponseMapper.ToResponse(recipe));
    }
}