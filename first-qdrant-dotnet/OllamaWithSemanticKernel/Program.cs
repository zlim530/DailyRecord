using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Ollama;
using System.Text;

//string modelName = "deepseek-r1:1.5b";
//using var ollama = new OllamaApiClient(baseUri: new Uri("http://127.0.0.1:11434/api"));
//https://www.cnblogs.com/wucy/p/18400124/csharp-ollama: C# 整合 Ollama 实现本地 LLMs 调用

using HttpClient httpClient = new HttpClient(new RedirectingHandler());
httpClient.Timeout = TimeSpan.FromSeconds(120);

var kernelBuilder = Kernel.CreateBuilder() // nuget Microsoft.SemanticKernel.Abstractions v1.45.0:https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.kernel.createbuilder?view=semantic-kernel-dotnet
    .AddOpenAIChatCompletion(              // nuget Microsoft.SemanticKernel.Connectors.OpenAI:https://learn.microsoft.com/zh-cn/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-OpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp
       modelId: "deepseek-r1:1.5b",
       apiKey: "ollama",
       httpClient: httpClient);
Kernel kernel = kernelBuilder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
//ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var history = new ChatHistory();
string? userInput;
do
{
Console.Write("User > ");
userInput = Console.ReadLine();
history.AddUserMessage(userInput!);

var result = chatCompletionService.GetStreamingChatMessageContentsAsync(
    history,
    executionSettings: openAIPromptExecutionSettings,
    kernel: kernel);
string fullMessage = "";
System.Console.Write("Assistant > ");
await foreach (var content in result)
{
System.Console.Write(content.Content);
fullMessage += content.Content;
}
System.Console.WriteLine();

history.AddAssistantMessage(fullMessage);
} while (userInput is not null);


public class RedirectingHandler : HttpClientHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uriBuilder = new UriBuilder(request.RequestUri!) { Scheme = "http", Host = "localhost", Port = 11434 };
        //对话模型
        if (request!.RequestUri!.PathAndQuery.Contains("v1/chat/completions"))
        {
            uriBuilder.Path = "/v1/chat/completions";
            request.RequestUri = uriBuilder.Uri;
        }
        //嵌入模型
        if (request!.RequestUri!.PathAndQuery.Contains("v1/embeddings"))
        {
            uriBuilder.Path = "/v1/embeddings";
            request.RequestUri = uriBuilder.Uri;
        }
        return base.SendAsync(request, cancellationToken);
    }
}


#region 单轮对话
//Console.WriteLine("开始对话");
//string userInput = "";
//do
//{
//    Console.WriteLine("User:");
//    userInput = Console.ReadLine()!;
//    var enumerable = ollama.Completions.GenerateCompletionAsync(modelName, userInput);
//    Console.WriteLine("Agent:");
//    await foreach (var response in enumerable)
//    {
//        Console.Write($"{response.Response}");
//    }
//    Console.WriteLine();

//} while (!string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase));
//Console.WriteLine("对话结束");
#endregion

#region 多轮对话
//Console.WriteLine("开始对话");
//string userInput = "";
//List<Message> messages = [];
//do
//{
//    //只取最新的五条消息
//    messages = messages.TakeLast(5).ToList();
//    Console.WriteLine("User:");
//    userInput = Console.ReadLine()!;
//    //加入用户消息
//    messages.Add(new Message(MessageRole.User, userInput, null, null));
//    var enumerable = ollama.Chat.GenerateChatCompletionAsync(modelName, messages, stream: true);
//    Console.WriteLine("Agent:");
//    StringBuilder builder = new();
//    await foreach (var response in enumerable)
//    {
//        string content = response.Message.Content;
//        builder.AppendLine(content);
//        Console.Write(content);
//    }
//    //加入机器消息
//    messages.Add(new Message(MessageRole.Assistant, builder.ToString(), null, null));
//    Console.WriteLine();

//} while (!string.Equals(userInput, "exit", StringComparison.OrdinalIgnoreCase));
//Console.WriteLine("对话结束");
#endregion
