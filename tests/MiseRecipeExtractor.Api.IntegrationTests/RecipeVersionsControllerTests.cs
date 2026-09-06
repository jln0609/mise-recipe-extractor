using System.Net;
using System.Net.Http.Json;
using MiseRecipeExtractor.Api.Dtos;

namespace MiseRecipeExtractor.Api.IntegrationTests;

public class RecipeVersionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public RecipeVersionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }
    
        [Fact]
    public async Task CreateAdjustedVersion_CreatesNewVersionWithIngredientsAndSteps()
    {
        // arrange
        var createRequest = new CreateRecipeRequest
        {
            Platform = "Xiaohongshu",
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork"
        };
        
        HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/recipes", createRequest);
        RecipeResponse? created = await postResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(created);
        
        var adjustedRequest = new CreateAdjustedVersionRequest
        {
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork (adjusted)",
            Ingredients = new List<IngredientDto>
            {
                new()
                {
                    NameOriginal = "五花肉",
                    NameTranslated = "Pork belly",
                    QuantityOriginalText = "500g",
                    QuantityAmount = 500,
                    QuantityUnit = "g",
                    QuantityConfidence = "Unspecified"
                }
            },
            Steps = new List<StepDto>
            {
                new()
                {
                    Order = 1,
                    TextOriginal = "将五花肉切块，冷水下锅焯水",
                    TextTranslated = "Cut pork belly into chunks, blanch in cold water",
                    OrderIsInferred = false
                }
            }
        };
        
        // act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/recipes/{created.Id}/versions", adjustedRequest);
        
        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        RecipeResponse? result = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(2, result.CurrentVersionNumber);
        Assert.Equal("Adjusted", result.Status);
        Assert.Equal("Braised Pork (adjusted)", result.TitleTranslated);
    }

    [Fact]
    public async Task CreateAdjustedVersion_PersistsAcrossSeparateGet()
    {
        // arrange
        var createRequest = new CreateRecipeRequest
        {
            Platform = "Xiaohongshu",
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork"
        };

        HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/recipes", createRequest);
        RecipeResponse? created = await postResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(created);

        var adjustedRequest = new CreateAdjustedVersionRequest
        {
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork (adjusted)",
            Ingredients = new List<IngredientDto>
            {
                new()
                {
                    NameOriginal = "五花肉",
                    QuantityOriginalText = "500g",
                    QuantityAmount = 500,
                    QuantityUnit = "g",
                    QuantityConfidence = "Unspecified"
                }
            },
            Steps = new List<StepDto>
            {
                new() { Order = 1, TextOriginal = "焯水", OrderIsInferred = false }
            }
        };

        await _client.PostAsJsonAsync($"/api/recipes/{created.Id}/versions", adjustedRequest);

        // act
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/recipes/{created.Id}");

        // assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        RecipeResponse? retrieved = await getResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(2, retrieved.CurrentVersionNumber);
        Assert.Equal("Adjusted", retrieved.Status);
        Assert.Equal("Braised Pork (adjusted)", retrieved.TitleTranslated);
    }

    [Fact]
    public async Task CreateAdjustedVersion_NonExistentId_ReturnsNotFound()
    {
        // arrange
        var adjustedRequest = new CreateAdjustedVersionRequest
        {
            TitleOriginal = "无",
            Ingredients = new List<IngredientDto>(),
            Steps = new List<StepDto>()
        };
        
        // act
        HttpResponseMessage response = await _client.PostAsJsonAsync($"/api/recipes/{Guid.NewGuid()}/versions", adjustedRequest);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}