using Microsoft.AspNetCore.Mvc;
using MiseRecipeExtractor.Api.Dtos;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.UseCases;

namespace MiseRecipeExtractor.Api.Controllers;

[ApiController]
[Route("api/recipes/{id}/versions")]
public class RecipeVersionsController(CreateAdjustedVersionCommand command, IRecipeRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeResponse>> CreateAdjustedVersion(Guid id, CreateAdjustedVersionRequest request)
    {
        var (title, ingredients, steps) = CreateAdjustedVersionRequestMapper.ToDomain(request);

        Recipe? recipe = await command.ExecuteAsync(id, title, ingredients, steps);
        if (recipe == null)
        {
            return NotFound();
        }
        
        return Ok(RecipeResponseMapper.ToResponse(recipe));
    }

    [HttpGet]
    public async Task<ActionResult<List<RecipeVersionResponse>>> GetAll(Guid id)
    {
        Recipe? recipe = await repository.GetByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }
        
        List<RecipeVersionResponse> responses = recipe.Versions.OrderBy(v => v.VersionNumber)
            .Select(RecipeVersionResponseMapper.ToResponse).ToList();

        return Ok(responses);
    }

    [HttpGet("{versionNumber:int}")]
    public async Task<ActionResult<RecipeVersionResponse>> GetByVerionNumber(Guid id, int versionNumber)
    {
        Recipe? recipe = await repository.GetByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }
        
        RecipeVersion? version = recipe.Versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (version == null)
        {
            return NotFound();
        }
        
        return Ok(RecipeVersionResponseMapper.ToResponse(version));
    }
}