using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObsidianRag.Clients;
using ObsidianRag.DB;
using ObsidianRag.Models;
using ObsidianRag.Services;
using Pgvector.EntityFrameworkCore;

namespace ObsidianRag.Controllers;


[ApiController]
public class SearchController : ControllerBase
{
    private readonly ILogger<ObsidianDataIngestionService> _logger;
    private readonly IDbContextFactory<ObsidianDbContext> _db;
    private readonly IEmbeddingClient _client;

    public SearchController(ILogger<ObsidianDataIngestionService> logger, IDbContextFactory<ObsidianDbContext> db, IEmbeddingClient client)
    {
        _logger = logger;
        _db = db;
        _client = client;
    }

    [HttpPost("Search")]
    public async Task<Response> GetResponse([FromBody] SearchRequest request)
    {
        var queryEmbedding = await _client.EmbedAsync(request.Search);
        
        await using ObsidianDbContext db = await _db.CreateDbContextAsync();
        
        var results = await db.Notes
            .OrderBy(x=>x.Embedding.CosineDistance(queryEmbedding))
            .Take(3)
            .ToListAsync();

        var searchResults = new List<SearchResultItem>();

        foreach (var result in results)
        {
            searchResults.Add(new SearchResultItem
            {
                SegmentId = result.Id,
                Content = result.Content
            });
        }

        return new Response
        {
            Results = searchResults,
        };
    }
}