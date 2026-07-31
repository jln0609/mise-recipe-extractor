using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Step
{
    Guid Id { get; set; }
    public int Order { get; set; }
    public LocalizedText Text { get; set; } = null!;
    public int? DurationSeconds { get; set; }
    public bool OrderIsInferred { get; set; }
}