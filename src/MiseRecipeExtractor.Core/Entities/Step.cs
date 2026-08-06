using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Step
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Order { get; set; }
    public LocalizedText Text { get; init; } = null!;
    public int? DurationSeconds { get; set; }
    public bool OrderIsInferred { get; set; }
}