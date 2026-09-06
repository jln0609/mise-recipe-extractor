using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiseRecipeExtractor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWarningsToRecipeVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Warnings",
                table: "RecipeVersions",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Warnings",
                table: "RecipeVersions");
        }
    }
}
