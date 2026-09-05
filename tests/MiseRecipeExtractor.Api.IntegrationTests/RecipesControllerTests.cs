using System.Net;
using System.Net.Http.Json;
using MiseRecipeExtractor.Api.Dtos;

namespace MiseRecipeExtractor.Api.IntegrationTests;

public class RecipesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    
    public RecipesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetById_ReturnsRecipeCreatedViaPost()
    {
        // arrange
        var createRequest = new CreateRecipeRequest
        {
            Platform = "Xiaohongshu",
            SourceUrl = "https://example.com/post/456",
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork"
        };

        HttpResponseMessage postResponse = await _client.PostAsJsonAsync("/api/recipes", createRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        
        RecipeResponse? created = await postResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(created);
        
        // act
        HttpResponseMessage getResponse = await _client.GetAsync($"/api/recipes/{created.Id}");
        
        // assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        RecipeResponse? retrieved = await getResponse.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal("Xiaohongshu", retrieved.Platform);
        Assert.Equal("https://example.com/post/456", retrieved.SourceUrl);
        Assert.Equal(1, retrieved.CurrentVersionNumber);
        Assert.Equal("红烧肉", retrieved.TitleOriginal);
        Assert.Equal("Braised Pork", retrieved.TitleTranslated);
        Assert.Equal("Draft", retrieved.Status);
    }

    [Fact]
    public async Task GetById_NonExistentId_ReturnsNotFound()
    {
        // act
        HttpResponseMessage response = await _client.GetAsync($"/api/recipes/{Guid.NewGuid()}");
        
        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsAllPersistedRecipes()
    {
        // arrange
        var firstRequest = new CreateRecipeRequest()
        {
            Platform = "Xiaohongshu",
            TitleOriginal = "红烧肉",
            TitleTranslated = "Braised Pork"
        };
        var secondRequest = new CreateRecipeRequest()
        {
            Platform = "Xiaohongshu",
            TitleOriginal = "麻婆豆腐",
            TitleTranslated = "Mapo Tofu"
        };
        
        HttpResponseMessage firstPost = await _client.PostAsJsonAsync("/api/recipes", firstRequest);
        HttpResponseMessage secondPost = await _client.PostAsJsonAsync("/api/recipes", secondRequest);
        
        RecipeResponse? first = await firstPost.Content.ReadFromJsonAsync<RecipeResponse>();
        RecipeResponse? second = await secondPost.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(first);
        Assert.NotNull(second);
        
        // act
        HttpResponseMessage getAllResponse = await _client.GetAsync($"/api/recipes");
        
        // assert
        Assert.Equal(HttpStatusCode.OK, getAllResponse.StatusCode);
        
        List<RecipeResponse>? all = await getAllResponse.Content.ReadFromJsonAsync<List<RecipeResponse>>();
        Assert.NotNull(all);
        Assert.Contains(all, r => r.Id == first.Id && r.TitleTranslated == "Braised Pork");
        Assert.Contains(all, r => r.Id == second.Id && r.TitleTranslated == "Mapo Tofu");
    }
}