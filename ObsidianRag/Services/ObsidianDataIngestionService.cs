using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ObsidianRag.Clients;
using ObsidianRag.DB;
using ObsidianRag.DB.Models;

namespace ObsidianRag.Services;

public class ObsidianDataIngestionService : BackgroundService
{
    private readonly ILogger<ObsidianDataIngestionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<ObsidianDbContext> _db;
    private readonly IEmbeddingClient _client;

    public ObsidianDataIngestionService(ILogger<ObsidianDataIngestionService> logger, IConfiguration configuration, IDbContextFactory<ObsidianDbContext> db, IEmbeddingClient client)
    {
        _logger = logger;
        _configuration = configuration;
        _db = db;
        _client = client;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await Task.Yield();
            
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessStorageAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

        }
        catch when (stoppingToken.IsCancellationRequested) { }
        catch (Exception e)
        {
            _logger.LogError(e, "Exeption durning obsidian data ingestion");
        }
        
    }
    
    private async Task ProcessStorageAsync(CancellationToken cancellationToken)
    {
        await using ObsidianDbContext db = await _db.CreateDbContextAsync(cancellationToken);
        
        var files = Directory.GetFiles(_configuration["ObsidianStoragePath"], "*.md", SearchOption.AllDirectories);
        _logger.LogTrace($"Find {files.Length} files");
        foreach (var file in files)
        {
            var title = Path.GetFileNameWithoutExtension(file);
            _logger.LogTrace($"File {title} start scanning");
            var text = await File.ReadAllTextAsync(file);
            List<string> tags  = ExtractTags(text);
            _logger.LogTrace($"Finx {tags.Count} tags");
            text = RemoveMetadata(text);
            var segments = SplitIntoSegments(text);
            _logger.LogTrace($"Create {segments.Count} segments");

            List<Note> notes = [];
            
            foreach (var segment in segments)
            {
                //TODO: А как удалить то что уже устарело? 
                if (await db.Notes.AnyAsync(s => s.Content == segment, cancellationToken: cancellationToken)) continue;
                var embedding = await _client.EmbedAsync(segment);
                notes.Add(
                new Note
                {
                    Title = title,
                    Content = segment,
                    Tags = tags,
                    Embedding = embedding
                });
            }
            
            await db.Notes.AddRangeAsync(notes, cancellationToken);
            
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private List<string> ExtractTags(string content)
    {
        var metadataMatch = Regex.Match(
            content,
            @"^---\s*(.*?)\s*---",
            RegexOptions.Singleline
        );

        List<string> tags = [];

        if (!metadataMatch.Success) return tags;
        
        var yaml = metadataMatch.Groups[1].Value;
        
        var tagsMatch = Regex.Match(
            yaml,
            @"tags:\s*\[(.*?)\]"
        );

        if (tagsMatch.Success)
        {
            tags = tagsMatch.Groups[1].Value
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        return tags;
    }
    
    private string RemoveMetadata(string content) =>
        Regex.Replace(content, @"^---\s.*?---\s*", "", RegexOptions.Singleline);

    private List<string> SplitIntoSegments(string text, int maxChars = 500)
    {
        var segments = new List<string>();
        var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var para in paragraphs)
        {
            if (para.Length <= maxChars)
                segments.Add(para);
            else
            {
                // Делим длинные параграфы
                for (int i = 0; i < para.Length; i += maxChars)
                {
                    segments.Add(para.Substring(i, Math.Min(maxChars, para.Length - i)));
                }
            }
        }
        return segments;
    }
}