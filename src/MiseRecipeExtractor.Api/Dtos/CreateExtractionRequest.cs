namespace MiseRecipeExtractor.Api.Dtos;

public class CreateExtractionRequest
{
    public List<IFormFile> Images { get; set; } = new();
    public string Platform { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
}
