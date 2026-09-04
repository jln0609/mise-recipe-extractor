using System.Net;
using System.Text;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.ValueObjects;


namespace MiseRecipeExtractor.AI.Tests;

public class AnthropicRecipeExtractorTests
{
    private static readonly byte[] FakePngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    
    [Fact]
    public async Task ExtractAsync_UnsupportedImageFormat_ThrowNotSupportedException()
    {
        // arrange
        var extractor = new AnthropicRecipeExtractor(new HttpClient());
        byte[] notAnImage = { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        
        // act/assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => extractor.ExtractAsync(new List<byte[]>() { notAnImage }));
    }
    
    [Fact]
    public async Task ExtractAsync_ValidToolUseResponse_ReturnsCorrectlyMappedExtractionResult()
    {
        // arrange
        var extractor = CreateExtractorWithFixture("AnthropicValidToolUseResponse.json");
        
        // act
        ExtractionResult result = await extractor.ExtractAsync(new List<byte[]>() { FakePngBytes });
        
        // assert
        Assert.Equal("zh", result.DetectedSourceLanguage);
        Assert.Single(result.Warnings);
        Assert.Equal("Bottom of the image was slightly cut off.", result.Warnings[0]);
        
        RecipeVersion version = result.ExtractedVersion;
        Assert.Equal("红烧肉", version.Title.Original);
        Assert.Equal("Braised Pork", version.Title.Translated);
        
        Assert.Equal(2, version.Ingredients.Count);
        
        Ingredient pork = version.Ingredients[0];
        Assert.Equal("猪肉", pork.Name.Original);
        Assert.Equal("Pork", pork.Name.Translated);
        Assert.Equal(500, pork.Quantity.Amount);
        Assert.Equal("g", pork.Quantity.Unit);
        Assert.Equal(ConfidenceLevel.Explicit, pork.Quantity.Confidence);
        
        Ingredient salt = version.Ingredients[1];
        Assert.Equal("适量", salt.Quantity.OriginalText);
        Assert.Null(salt.Quantity.Amount);
        Assert.Equal(ConfidenceLevel.Unspecified, salt.Quantity.Confidence);
        
        Assert.Equal(2, version.Steps.Count);
        Assert.Equal("切块", version.Steps[0].Text.Original);
        Assert.False(version.Steps[0].OrderIsInferred);
        Assert.Equal(3600, version.Steps[1].DurationSeconds);
    }
    
    [Fact]
    public async Task ExtractAsync_NonSuccessStatusCode_ThrowsHttpRequestException()
    {
        var extractor = CreateExtractorWithFixture("AnthropicErrorStatusResponse.json", HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => extractor.ExtractAsync(new List<byte[]> { FakePngBytes }));
    }
    
    [Fact]
    public async Task ExtractAsync_MissingContentArray_ThrowsInvalidOperationException()
    {
        var extractor = CreateExtractorWithFixture("AnthropicMissingContentResponse.json");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => extractor.ExtractAsync(new List<byte[]> { FakePngBytes }));
    }
    
    [Fact]
    public async Task ExtractAsync_NoToolUseBlock_ThrowsInvalidOperationException()
    {
        var extractor = CreateExtractorWithFixture("AnthropicNoToolUseResponse.json");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => extractor.ExtractAsync(new List<byte[]> { FakePngBytes }));
    }
    
    [Fact]
    public async Task ExtractAsync_MissingInput_ThrowsInvalidOperationException()
    {
        var extractor = CreateExtractorWithFixture("AnthropicMissingInputResponse.json");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => extractor.ExtractAsync(new List<byte[]> { FakePngBytes }));
    }
    
    [Fact]
    public async Task ExtractAsync_UnrecognizedConfidenceString_FallsBackToUnspecified()
    {
        // arrange
        var extractor = CreateExtractorWithFixture("AnthropicUnrecognizedConfidenceResponse.json");
        
        // act
        ExtractionResult result = await extractor.ExtractAsync(new List<byte[]>() { FakePngBytes });
        
        // assert
        Ingredient ingredient = Assert.Single(result.ExtractedVersion.Ingredients);
        Assert.Equal(ConfidenceLevel.Unspecified, ingredient.Quantity.Confidence);
    }
    
    // helper

    private static AnthropicRecipeExtractor CreateExtractorWithFixture(
        string fixtureFileName, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        string responseJson = File.ReadAllText(
            Path.Combine(AppContext.BaseDirectory, "SampleResponses", fixtureFileName));
        
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseJson, Encoding.UTF8, "application/json")
            });
        
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.anthropic.com/") };
        return new AnthropicRecipeExtractor(httpClient);
    }
}