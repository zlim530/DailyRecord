namespace RAGDemo.RERANK
{
    public interface IRerankService
    {
        Task<IReadOnlyList<RerankScore>> RerankAsync(string query, IReadOnlyList<string> documents, CancellationToken ct = default);
    }
}

public record RerankScore(int Index, float Score);