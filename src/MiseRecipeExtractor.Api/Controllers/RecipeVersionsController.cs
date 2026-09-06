using Microsoft.AspNetCore.Mvc;
using MiseRecipeExtractor.Api.Dtos;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.UseCases;

namespace MiseRecipeExtractor.Api.Controllers;

[ApiController]
[Route("api/recipes/{id}/versions")]
public class RecipeVersionsController(CreateAdjustedVersionCommand command) : ControllerBase
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
}