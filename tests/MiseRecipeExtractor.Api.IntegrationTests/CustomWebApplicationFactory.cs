using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiseRecipeExtractor.Core.Interfaces;
using MiseRecipeExtractor.Infrastructure;

namespace MiseRecipeExtractor.Api.IntegrationTests;

public class CustomWebApplicationFactory: WebApplicationFactory<Program>
{
    
    private readonly SqliteConnection _connection = new("DataSource=:memory:");
    
    public CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Anthropic__ApiKey", "test-placeholder-key");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? dbContextDescriptor =
                services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<RecipeDbContext>));
            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);
            
            _connection.Open();

            services.AddDbContext<RecipeDbContext>(options => 
            options.UseSqlite(_connection, sqliteOptions =>
                sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
            
            ServiceDescriptor? recipeExtractorDescriptor
                = services.SingleOrDefault(d => d.ServiceType == typeof(IRecipeExtractor));
            if (recipeExtractorDescriptor is not null)
                services.Remove(recipeExtractorDescriptor);

            services.AddScoped<IRecipeExtractor, FakeRecipeExtractor>();
            
            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            RecipeDbContext db = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }

}