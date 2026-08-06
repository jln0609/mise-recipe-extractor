using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiseRecipeExtractor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source_ExtractedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source_OriginalLanguage = table.Column<string>(type: "TEXT", nullable: false),
                    Source_Platform = table.Column<string>(type: "TEXT", nullable: false),
                    Source_SourceUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecipeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Title_Original = table.Column<string>(type: "TEXT", nullable: false),
                    Title_Translated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeVersions_Recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    RecipeVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Name_Original = table.Column<string>(type: "TEXT", nullable: false),
                    Name_Translated = table.Column<string>(type: "TEXT", nullable: true),
                    Quantity_Amount = table.Column<double>(type: "REAL", nullable: true),
                    Quantity_Confidence = table.Column<int>(type: "INTEGER", nullable: false),
                    Quantity_OriginalText = table.Column<string>(type: "TEXT", nullable: false),
                    Quantity_Unit = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_RecipeVersions_RecipeVersionId",
                        column: x => x.RecipeVersionId,
                        principalTable: "RecipeVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    OrderIsInferred = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecipeVersionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Text_Original = table.Column<string>(type: "TEXT", nullable: false),
                    Text_Translated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Steps_RecipeVersions_RecipeVersionId",
                        column: x => x.RecipeVersionId,
                        principalTable: "RecipeVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_RecipeVersionId",
                table: "Ingredients",
                column: "RecipeVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeVersions_RecipeId",
                table: "RecipeVersions",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_Steps_RecipeVersionId",
                table: "Steps",
                column: "RecipeVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "Steps");

            migrationBuilder.DropTable(
                name: "RecipeVersions");

            migrationBuilder.DropTable(
                name: "Recipes");
        }
    }
}
