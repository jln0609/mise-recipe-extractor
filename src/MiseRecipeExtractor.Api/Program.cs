using Microsoft.EntityFrameworkCore;
using MiseRecipeExtractor.AI;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Core.UseCases;
using MiseRecipeExtractor.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);
// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

string connectionString = builder.Configuration.GetConnectionString("RecipeDb")
                          ?? throw new InvalidOperationException("ConnectionStrings:ReipeDb is not configured.");

builder.Services.AddDbContext<RecipeDbContext>(options =>
    options.UseSqlite(connectionString, sqliteOptions => 
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
builder.Services.AddScoped<CreateAdjustedVersionCommand>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
