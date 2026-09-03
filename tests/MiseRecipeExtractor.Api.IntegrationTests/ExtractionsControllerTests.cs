using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MiseRecipeExtractor.Api.Dtos;

namespace MiseRecipeExtractor.Api.IntegrationTests;

public class ExtractionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ExtractionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostExtraction_PersistsAndReturnsRecipe()
    {
        // arrange
        using var content = new MultipartFormDataContent();
        byte[] fakeImageBytes = { 0x01, 0x02, 0x03 };
        var imageContent = new ByteArrayContent(fakeImageBytes);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(imageContent, "Images", "test.png");
        content.Add(new StringContent("Xiaohongshu"), "Platform");
        content.Add(new StringContent("https://example.com/post/123"), "SourceUrl");

        // act
        HttpResponseMessage response = await _client.PostAsync("api/extractions", content);

        // assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        RecipeResponse? recipe = await response.Content.ReadFromJsonAsync<RecipeResponse>();
        Assert.NotNull(recipe);
        Assert.Equal("Xiaohongshu", recipe.Platform);
        Assert.Equal("https://example.com/post/123", recipe.SourceUrl);
        Assert.Equal(1, recipe.CurrentVersionNumber);
        Assert.Equal("测试食谱", recipe.TitleOriginal);
        Assert.Equal("Test Recipe", recipe.TitleTranslated);
        Assert.Equal("Draft", recipe.Status);
    }
}