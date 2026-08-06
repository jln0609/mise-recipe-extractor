using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Ingredient
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public LocalizedText Name { get; init; } = null!;
    public Quantity Quantity { get; init; } = null!;
    public string? Notes { get; set; }
}