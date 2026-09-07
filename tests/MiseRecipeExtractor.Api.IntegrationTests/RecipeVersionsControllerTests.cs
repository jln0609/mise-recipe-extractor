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

    [Fact]
    public async Task GetAllVersions_ReturnsBothVersionsWithCorrectData()
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
                new() { Order = 1, TextOriginal = "焯水", OrderIsInferred = false }
            }
        };
        await _client.PostAsJsonAsync($"/api/recipes/{created.Id}/versions", adjustedRequest);

        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{created.Id}/versions");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        List<RecipeVersionResponse>? versions = await response.Content.ReadFromJsonAsync<List<RecipeVersionResponse>>();
        Assert.NotNull(versions);
        Assert.Equal(2, versions.Count);
        
        RecipeVersionResponse v1 = versions.Single(v => v.VersionNumber == 1);
        Assert.Equal("Braised Pork", v1.TitleTranslated);
        Assert.Empty(v1.Ingredients);
        Assert.Empty(v1.Steps);
        
        RecipeVersionResponse v2 = versions.Single(v => v.VersionNumber == 2);
        Assert.Equal("Braised Pork (adjusted)", v2.TitleTranslated);
        Assert.Single(v2.Ingredients);
        Assert.Single(v2.Steps);
    }
    
    [Fact]
    public async Task GetAllVersions_NonExistentRecipeId_ReturnsNotFound()
    {
        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{Guid.NewGuid()}/versions");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetVersionByNumber_ReturnsOriginalVersionUntouchedAfterAdjustment()
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
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{created.Id}/versions/1");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        RecipeVersionResponse? version1 = await response.Content.ReadFromJsonAsync<RecipeVersionResponse>();
        Assert.NotNull(version1);
        Assert.Equal(1, version1.VersionNumber);
        Assert.Equal("Draft", version1.Status);
        Assert.Equal("Braised Pork", version1.TitleTranslated);
        Assert.Empty(version1.Ingredients);
        Assert.Empty(version1.Steps);
    }

    [Fact]
    public async Task GetVersionByNumber_NonExistentVersionNumber_ReturnsNotFound()
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

        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{created.Id}/versions/99");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [Fact]
    public async Task GetVersionByNumber_NonExistentRecipeId_ReturnsNotFound()
    {
        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{Guid.NewGuid()}/versions/1");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}