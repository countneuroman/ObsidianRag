using Microsoft.EntityFrameworkCore;
using ObsidianRag.Clients;
using ObsidianRag.DB;
using ObsidianRag.Services;

namespace ObsidianRag;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        
        builder.Services.AddHttpLogging();
        
        builder.Services.AddHttpClient<IEmbeddingClient, OllamaEmbeddingClient>(client =>
        {
            client.BaseAddress = new Uri("http://host.docker.internal:11434");
        });

        builder.Services.AddPooledDbContextFactory<ObsidianDbContext>(optons =>
        {
            optons.UseNpgsql(builder.Configuration["ConnectionString"], opts =>opts.UseVector());
        });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        //Register data ingestion services
        builder.Services.AddSingleton<ObsidianDataIngestionService>();
        builder.Services.AddHostedService(sp => sp.GetService<ObsidianDataIngestionService>());
        
        var app = builder.Build();
        
        //Run Migrations
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ObsidianDbContext>();
            db.Database.Migrate();
        }
        
        app.UseHttpLogging();
        
        app.MapControllers();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        app.Run();
    }
}