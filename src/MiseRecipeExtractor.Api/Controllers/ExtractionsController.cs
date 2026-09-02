using Microsoft.AspNetCore.Mvc;
using MiseRecipeExtractor.Api.Dtos;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.UseCases;

namespace MiseRecipeExtractor.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExtractionsController(ExtractAndCreateRecipeCommand extractCommand) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RecipeResponse>> Create([FromForm] CreateExtractionRequest request)
    {
        List<byte[]> images = new List<byte[]>();
        foreach (var file in request.Images)
        {
            using MemoryStream stream = new MemoryStream();
            await file.CopyToAsync(stream);
            images.Add(stream.ToArray());
        }

        Recipe recipe = await extractCommand.ExecuteAsync(images, request.Platform, request.SourceUrl);
        
        return CreatedAtAction("GetById", "Recipes", new { id = recipe.Id }, RecipeResponseMapper.ToResponse(recipe));
    }
}