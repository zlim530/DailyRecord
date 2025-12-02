using Qdrant.Client;
using Qdrant.Client.Grpc;

var client = new QdrantClient("localhost", 6334);
//Working with collections
//Once a client has been created, create a new collection
//await client.CreateCollectionAsync("my_first_collection", new VectorParams { Size = 100, Distance = Distance.Cosine});

//Insert vectors into a collection
var random = new Random();
var points = Enumerable.Range(1, 100).Select(i => new PointStruct 
{
    Id = (ulong)i,
    Vectors = Enumerable.Range(1,100).Select(_ => (float)random.NextDouble()).ToArray(),
    Payload =
    {
        ["color"] = "red",
        ["rand_number"] = i % 10
    }
}).ToList();

//var updateResult = await client.UpsertAsync("my_first_collection",points);

//Search for similar vectors
var queryVector = Enumerable.Range(1, 100).Select(_ => (float)random.NextDouble()).ToArray();

var searchPoints = await client.SearchAsync("my_first_collection", queryVector, limit:5);

Console.WriteLine(searchPoints.Count);// 5
//Console.WriteLine(searchPoints);

//Search for similar vectors with filtering condition
var filter = Conditions.Range("rand_number", new Qdrant.Client.Grpc.Range { Gte = 3});
var conditionSearchPoints = await client.SearchAsync("my_first_collection", queryVector, filter: filter, limit:10);
Console.WriteLine(conditionSearchPoints.Count);// 10
Console.WriteLine(conditionSearchPoints);
