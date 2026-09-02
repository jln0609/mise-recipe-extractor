using Microsoft.EntityFrameworkCore;
using MiseRecipeExtractor.AI;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.UseCases;
using MiseRecipeExtractor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlite("Data Source=recipes.db", sqliteOptions => 
        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddScoped<IRecipeRepository, EfRecipeRepository>();

var anthropicApiKey = builder.Configuration["Anthropic:ApiKey"]
    ?? throw new InvalidOperationException(
        "Anthropic:ApiKey is not configured. Run: dotnet user-secrets set \"Anthropic:ApiKey\" \"your-key\"");

builder.Services.AddHttpClient<IRecipeExtractor, AnthropicRecipeExtractor>(client =>
    {
        client.BaseAddress = new Uri("https://api.anthropic.com/");
        client.DefaultRequestHeaders.Add("x-api-key", anthropicApiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    });

builder.Services.AddScoped<ExtractAndCreateRecipeCommand>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
