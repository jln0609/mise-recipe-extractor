using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Core.Entities;

public class Ingredient(LocalizedText name, Quantity quantity, string? notes = null)
{
    public Guid Id { get;} = Guid.NewGuid();
    public LocalizedText Name { get; } = name;
    public Quantity Quantity { get; } = quantity;
    public string? Notes { get; set; } = notes;
}