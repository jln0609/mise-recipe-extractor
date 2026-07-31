using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Ingredient
{
    public Guid Id { get; set; }
    public LocalizedText Name { get; set; } = null!;
    public Quantity Quantity { get; set; } = null!;
    public string? Notes { get; set; }
}