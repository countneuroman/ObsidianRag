namespace ObsidianRag.Models;

public class SearchResultItem
{
    public long SegmentId { get; set; }
    public long NoteId { get; set; }
    public string? Content { get; set; }
}