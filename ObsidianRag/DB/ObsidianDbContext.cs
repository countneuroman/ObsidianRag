using Microsoft.EntityFrameworkCore;
using ObsidianRag.DB.Models;
using ObsidianRag.Models;

namespace ObsidianRag.DB;

public class ObsidianDbContext(DbContextOptions<ObsidianDbContext> options) : DbContext(options)
{
    public static class Defaults
    {
        public const string EmbeddingModel = "qwen3-embedding:8b";
        public const int EmbeddingDimensions = 4096;
    }

    public DbSet<Note> Notes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<NoteSegmentSearchResult>()
            .HasNoKey()
            .ToView(null);
        
        // Enable pgvector extension
        modelBuilder.HasPostgresExtension("vector");
        
        modelBuilder.Entity<Note>(entity =>
        {
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(4096)");
        });
        
        base.OnModelCreating(modelBuilder);
    }
}