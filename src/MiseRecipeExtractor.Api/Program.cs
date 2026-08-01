using MiseRecipeExtractor.Api.Fakes;
using MiseRecipeExtractor.Core.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// For testing:
builder.Services.AddSingleton<IRecipeRepository, InMemoryRecipeRepository>();
//

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
