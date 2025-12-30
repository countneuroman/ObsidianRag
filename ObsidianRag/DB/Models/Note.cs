using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace ObsidianRag.DB.Models;

[Table("notes")]
public class Note
{
    public int Id { get; set; }
    
    public string? Path { get; set; }
    
    public string Title { get; set; }
    
    public string Content { get; set; }

    public ICollection<string> Tags { get; set; } = new List<string>();
    
    public Vector Embedding { get; set; }
}