using Microsoft.EntityFrameworkCore;
using MiseRecipeExtractor.Core.Entities;
using MiseRecipeExtractor.Core.ValueObjects;

namespace MiseRecipeExtractor.Infrastructure;

public class RecipeDbContext(DbContextOptions<RecipeDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(recipe =>
        {
            recipe.ToTable("Recipes");
            recipe.HasKey(r => r.Id);
            recipe.ComplexProperty(r => r.Source);

            recipe.HasMany(r => r.Versions)
                .WithOne()
                .HasForeignKey("RecipeId")
                .OnDelete(DeleteBehavior.Cascade);

            recipe.Ignore(r => r.CurrentVersion);
        });

        modelBuilder.Entity<RecipeVersion>(version =>
        {
            version.ToTable("RecipeVersions");
            version.HasKey(v => v.Id);
            version.ComplexProperty(v => v.Title);

            version.HasMany(v => v.Ingredients)
                .WithOne()
                .HasForeignKey("RecipeVersionId")
                .OnDelete(DeleteBehavior.Cascade);

            version.HasMany(v => v.Steps)
                .WithOne()
                .HasForeignKey("RecipeVersionId")
                .OnDelete(DeleteBehavior.Cascade);

            version.Property(v => v.Warnings);
        });

        modelBuilder.Entity<Ingredient>(ingredient =>
        {
            ingredient.ToTable("Ingredients");
            ingredient.HasKey(i => i.Id);
            ingredient.ComplexProperty(i => i.Name);
            ingredient.ComplexProperty(i => i.Quantity);
        });

        modelBuilder.Entity<Step>(step =>
        {
            step.ToTable("Steps");
            step.HasKey(s => s.Id);
            step.ComplexProperty(s => s.Text);
        });
    }
}