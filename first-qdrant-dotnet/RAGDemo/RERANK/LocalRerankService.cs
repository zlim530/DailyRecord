using System.Net.Http.Json;

namespace RAGDemo.RERANK
{
    /// <summary>
    /// 调用本地 http://localhost:5005/rerank Rerank 服务
    /// 使用 bge-reranker-v2-m3 ONNX 模型进行文档重排序
    /// https://huggingface.co/onnx-community/bge-reranker-v2-m3-ONNX
    /// </summary>
    public class LocalRerankService : IRerankService
    {
        private readonly HttpClient _http;

        public LocalRerankService(HttpClient http)
        {
            _http = http;
        }

        public async Task<IReadOnlyList<RerankScore>> RerankAsync(string query, IReadOnlyList<string> documents, CancellationToken ct = default)
        {
            var request = new
            {
                Query = query,
                Documents = documents
            };

            var resp = await _http.PostAsJsonAsync("/rerank", request, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<LocalRerankResponse>(ct) ?? throw new Exception("Empty rerank response");

            return json.Results.Select(r => new RerankScore(r.Index, r.Score))
                                .ToList();
        }

        private sealed class LocalRerankResponse
        {
            public List<RerankItem> Results { get; set; } = new();

            public sealed class RerankItem
            {
                public int Index { get; set; }
                public float Score { get; set; }
            }
        }


    }

}
