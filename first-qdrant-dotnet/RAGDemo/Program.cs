// See https://aka.ms/new-console-template for more information
using Azure.AI.OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.ClientModel;
using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");

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
                ["text"] = text,
                ["userId"] = "user_530"
            }
    };

    points.Add(point);
    Console.WriteLine($"✓ {text}");
}

// Upsert all points to Qdrant
await qdrantClient.UpsertAsync(collectionName, points);
Console.WriteLine($"\nStored {points.Count} embeddings in Qdrant\n");

// Generate embedding for query
var queryEmbeddingResult = await embeddingClient.GenerateEmbeddingAsync("哆啦A梦使用的3个秘密道具分别是什么？");
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