using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using RAGDemo.RERANK;
using System.ClientModel;
using System.Text.RegularExpressions;

Console.WriteLine("=== RAG Demo 启动 ===");
Console.WriteLine("集成技术: .NET 10 + Azure OpenAI + Qdrant + Local RERANK + GPT-5.1\n");

// read
string file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData", "doc.md");
string doc = await File.ReadAllTextAsync(file);

// slice
var segments = SplitToSegments(doc);

var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
var endpoint =
    "https://dockerplan.openai.azure.com/";
var deploymentName = "text-embedding-3-large"; //"text-embedding-v4"

if (string.IsNullOrEmpty(apiKey))
{
    Console.WriteLine("Error: AZURE_OPENAI_API_KEY environment variable not set.");
    return;
}

var client = new AzureOpenAIClient(
       new Uri(endpoint),
       new ApiKeyCredential(apiKey)
    );
var embeddingClient = client.GetEmbeddingClient(deploymentName);

var qdrantHost = "127.0.0.1";
var qdrantClient = new QdrantClient(qdrantHost, 6334);
var collectionName = "rag_demo_collection";

var sampleEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync(segments[0]);
var vectorSize = (ulong)sampleEmbeddingResult.Value.ToFloats().Length;

// Check if collection exists and compare vector size
bool needRecreate = false;
bool collectionExists = await qdrantClient.CollectionExistsAsync(collectionName);

if (collectionExists)
{
    var collectionInfo = await qdrantClient.GetCollectionInfoAsync(collectionName);
    var existingVectorSize = collectionInfo.Config.Params.VectorsConfig.Params.Size;

    if (existingVectorSize != vectorSize)
    {
        Console.WriteLine(
            $"Collection exists but vector size mismatch (existing: {existingVectorSize}, new: {vectorSize})");
        needRecreate = true;
    }
    else
    {
        Console.WriteLine(
            $"Collection '{collectionName}' already exists with matching vector size ({vectorSize})");
    }
}
else
{
    Console.WriteLine($"Collection '{collectionName}' does not exist");
    needRecreate = true;
}

if (needRecreate)
{
    if (collectionExists)
    {
        await qdrantClient.DeleteCollectionAsync(collectionName);
        Console.WriteLine($"Deleted existing collection '{collectionName}'");
    }

    await qdrantClient.CreateCollectionAsync(
        collectionName: collectionName,
        vectorsConfig: new VectorParams
        {
            Size = vectorSize,
            Distance = Distance.Cosine
        }
    );

    Console.WriteLine($"Created collection '{collectionName}'");
}

Console.WriteLine("Generating embeddings and storing in Qdrant.\n");

// Generate embeddings and insert into Qdrant
var points = new List<PointStruct>();

for (int i = 0; i < segments.Count; i++)
{
    var embeddingResult = await embeddingClient.GenerateEmbeddingAsync(segments[i]);
    var embedding = embeddingResult.Value.ToFloats().ToArray();

    var point = new PointStruct
    {
        Id = new PointId { Num = (ulong)i },
        Vectors = embedding,
        Payload =
            {
                ["text"] = segments[i],
                ["userId"] = "user_530"
            }
    };

    points.Add(point);
    //Console.WriteLine($"{segments[i]}");
}

// Upsert all points to Qdrant
await qdrantClient.UpsertAsync(collectionName, points);
Console.WriteLine($"\nStored {points.Count} embeddings in Qdrant\n");

// Generate embedding for query
var query = "哆啦A梦使用的3个秘密道具分别是什么?";
var queryEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync(query);
var queryEmbedding = queryEmbeddingResult.Value.ToFloats().ToArray();

// Search in Qdrant
var searchResults = await qdrantClient.SearchAsync(
    collectionName: collectionName,
    vector: queryEmbedding,
    limit: 20
);

for (int i = 0; i < searchResults.Count; i++)
{
    var result = searchResults[i];
    var text = result.Payload["text"].StringValue;
    var similarity = result.Score;

    if (similarity > 0.5)
    {
        Console.WriteLine($"\n{i + 1}. Similarity: {similarity:F4} ({similarity * 100:F2}%)");
        Console.WriteLine($"   Text: {text}");
    }

}

// 设置依赖注入
var services = new ServiceCollection();
services.AddHttpClient<IRerankService, LocalRerankService>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5005");
    c.Timeout = TimeSpan.FromSeconds(30);
});

var serviceProvider = services.BuildServiceProvider();
var rerankService = serviceProvider.GetRequiredService<IRerankService>();

Console.WriteLine("\n=== 开始 RERANK 重排序 ===");

// 提取搜索结果的文本用于重排序
var candidateTexts = searchResults
    .Where(r => r.Score > 0.5) // 只对相似度较高的结果进行重排序
    .Select(r => r.Payload["text"].StringValue)
    .ToList();

if (candidateTexts.Count == 0)
{
    Console.WriteLine("没有找到相关的文档片段");
    return;
}

Console.WriteLine($"对 {candidateTexts.Count} 个候选文档进行重排序");

// 使用 RERANK 服务重新排序
var rerankResults = await rerankService.RerankAsync(query, candidateTexts);

// 按重排序分数排序，取前5个
var topRerankResults = rerankResults
    .OrderByDescending(r => r.Score)
    .Take(5)
    .ToList();

Console.WriteLine("\n=== RERANK 结果 ===");
foreach (var result in topRerankResults)
{
    var text = candidateTexts[result.Index];
    Console.WriteLine($"重排序分数: {result.Score:F4} - {text.Substring(0, Math.Min(100, text.Length))}");
}

// 构建最终的上下文
var contextTexts = topRerankResults
    .Select(r => candidateTexts[r.Index])
    .ToList();

var contextString = string.Join("\n\n", contextTexts.Select((text, i) => $"片段 {i + 1}:\n{text}"));

// 使用 GPT 生成最终回答
Console.WriteLine("\n=== 生成最终回答 ===");

var chatClient = client.GetChatClient("gpt-5.1");

var systemMessage = ChatMessage.CreateSystemMessage(@"你是一位知识助手。请根据用户的问题和提供的相关片段生成准确、详细的回答。

要求：
1. 基于提供的片段内容回答问题
2. 如果片段中没有足够信息回答问题，请明确说明
3. 不要编造或推测片段中没有的信息
4. 回答要条理清晰，语言自然流畅");

var userMessage = ChatMessage.CreateUserMessage($@"用户问题: {query}

相关片段:
{contextString}

请基于上述内容回答问题。");

var messages = new List<ChatMessage> { systemMessage, userMessage };

var chatOptions = new ChatCompletionOptions
{
    Temperature = 0.1f
};

var response = await chatClient.CompleteChatAsync(messages, chatOptions);
var answer = response.Value.Content[0].Text;

Console.WriteLine($"\n问题: {query}");
Console.WriteLine($"\n回答:\n{answer}");

Console.WriteLine("\n=== RAG Demo 完成 ===");


/// <summary>
/// 分隔符
/// </summary>
static List<string> SplitToSegments(string text)
{
    var normalized = text
        .Replace("\r\n", "\n")
        .Replace("\r", "\n")
        .Replace("\u2028", "\n")   // 行分隔符
        .Replace("\u2029", "\n");  // 段落分隔符

    // 按"至少一个空行"切
    return Regex
        .Split(normalized, @"\n\s*\n+")
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrWhiteSpace(s))
        .ToList();
}