using ObsidianRag.Models;
using Pgvector;

namespace ObsidianRag.Clients;

public class OllamaEmbeddingClient : IEmbeddingClient
{
    private readonly HttpClient _httpClient;

    public OllamaEmbeddingClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Vector> EmbedAsync(string text, CancellationToken ct = default)
    {
        var request = new OllamaEmbeddingRequest
        {
            Model = "qwen3-embedding:8b",
            Prompt = text
        };

        var response = await _httpClient.PostAsJsonAsync("/api/embeddings", request, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: ct);
        return new Vector(Normalize(result!.Embedding));
    }

    public async Task<Vector[]> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        var tasks = texts.Select(t => EmbedAsync(t, ct));
        return await Task.WhenAll(tasks);
    }

    private static float[] Normalize(float[] v)
    {
        var norm = Math.Sqrt(v.Sum(x => x * x));
        return v.Select(x => (float)(x / norm)).ToArray();
    }
}

public interface IEmbeddingClient
{
    /// <summary>
    /// Получить embedding одного текста
    /// </summary>
    Task<Vector> EmbedAsync(string text, CancellationToken ct = default);

    /// <summary>
    /// Получить embeddings для списка текстов
    /// </summary>
    Task<Vector[]> EmbedBatchAsync(IEnumerable<string> texts, CancellationToken ct = default);
}