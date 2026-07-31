using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Step(int order, LocalizedText text, int? durationSeconds = null, bool orderIsInferred = false)
{
    public Guid Id { get; } = Guid.NewGuid();
    public int Order { get; set; } = order;
    public LocalizedText Text { get; } = text;
    public int? DurationSeconds { get; set; } = durationSeconds;
    public bool OrderIsInferred { get; set; } = orderIsInferred;
}