#pragma warning disable SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Functions;
using System.ClientModel;
using System.ComponentModel;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

#region Multi-agent routing
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
    .Build();

string ProgamManager = """
    You are a program manager which will take the requirement and create a plan for creating app. Program Manager understands the 
    user requirements and form the detail documents with requirements and costing. 
""";

string SoftwareEngineer = """
   You are Software Engieer, and your goal is develop web app using HTML and JavaScript (JS) by taking into consideration all
   the requirements given by Program Manager. 
""";

string Manager = """
    You are manager which will review software engineer code, and make sure all client requirements are completed.
     Once all client requirements are completed, you can approve the request by just responding "approve"
""";

#pragma warning disable SKEXP0110, SKEXP0001 // Rethrow to preserve stack details

ChatCompletionAgent ProgaramManagerAgent =
           new()
           {
               Instructions = ProgamManager,
               Name = "ProgaramManagerAgent",
               Kernel = kernel
           };

ChatCompletionAgent SoftwareEngineerAgent =
           new()
           {
               Instructions = SoftwareEngineer,
               Name = "SoftwareEngineerAgent",
               Kernel = kernel
           };

ChatCompletionAgent ProjectManagerAgent =
           new()
           {
               Instructions = Manager,
               Name = "ProjectManagerAgent",
               Kernel = kernel
           };

AgentGroupChat chat =
            new(ProgaramManagerAgent, SoftwareEngineerAgent, ProjectManagerAgent)
            {
                ExecutionSettings =
                    new()
                    {
                        TerminationStrategy =
                            new ApprovalTerminationStrategy()
                            {
                                Agents = [ProjectManagerAgent],
                                MaximumIterations = 6,
                            }
                    }
            };

string input = """
        
        I want to develop calculator app. 
        Keep it very simple. And get final approval from manager.
        """;

chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));
Console.WriteLine($"# {AuthorRole.User}: '{input}'");

await foreach (var content in chat.InvokeAsync())
{
    Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
}

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
//ChatCompletionAgent refundAgent = new()
//{
//    Name = "refundAgent",
//    Instructions =
//    """
//    Assist users with refund inquiries, including eligibility,
//    policies, processing, and status updates.
//    """,
//    Kernel = kernel,
//    Arguments = new(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true }) }),
//    // This setting must be set to true when using the ContextualFunctionProvider
//    UseImmutableKernel = true
//};
//#pragma warning restore SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
//#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。


//// Define the agent
//ChatCompletionAgent agent =
//    new()
//    {
//        Name = "SalesAssistant",
//        Instructions = "You are a sales assistant. Place orders for items the user requests.",
//        Kernel = kernel,
//        Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }),
//    };

//var agentPlugin = KernelPluginFactory.CreateFromFunctions("AgentPlugin",
//            [
//                AgentKernelFunctionFactory.CreateFromAgent(agent),
//                AgentKernelFunctionFactory.CreateFromAgent(refundAgent)
//            ]);


//// Invoke the agent and display the responses
//var responseItems = agent.InvokeAsync(new Microsoft.SemanticKernel.ChatMessageContent(AuthorRole.User, "Place an order for a black boot."));
//await foreach (ChatMessageContent responseItem in responseItems)
//{
//    WriteAgentChatMessage(responseItem);
//}


sealed class ApprovalTerminationStrategy : TerminationStrategy
{
    // Terminate when the final message contains the term "approve"
    protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> history, CancellationToken cancellationToken)
        => Task.FromResult(history[history.Count - 1].Content?.Contains("approve", StringComparison.OrdinalIgnoreCase) ?? false);
}
#endregion



#region 上下文函数选择的工作原理
/*
https://learn.microsoft.com/zh-cn/semantic-kernel/frameworks/agent/agent-contextual-function-selection?pivots=programming-language-csharp
上下文函数选择的工作原理
当代理配置有上下文函数选择时，它将利用矢量存储和嵌入生成器，以语义方式将当前聊天上下文（包括以前的消息和用户输入）与可用函数的说明和名称匹配。 然后，将最相关的函数在达到指定限制时推送给 AI 模型进行调用。

此机制对于能够访问大量插件或工具的代理特别有用，确保在每个步骤中只考虑上下文适当的操作。

用法示例
以下示例演示如何将代理配置为使用上下文函数选择。 代理被设置用于汇总客户评论，但每次调用仅向 AI 模型提供最相关的功能。 该方法 GetAvailableFunctions 有意包括相关和无关的函数，以突出显示上下文选择的优点。
*/
// Create an embedding generator for function vectorization
/*var embeddingGenerator = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")), new ApiKeyCredential(Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")))
    .GetEmbeddingClient("text-embedding-3-large")
    .AsIEmbeddingGenerator();

// Create kernel and register AzureOpenAI chat completion service
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
        Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
    .Build();

// Create a chat completion agent
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
ChatCompletionAgent agent = new()
{
    Name = "ReviewGuru",
    Instructions = "You are a friendly assistant that summarizes key points and sentiments from customer reviews. For each response, list available functions.",
    Kernel = kernel,
    Arguments = new(new PromptExecutionSettings { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: new FunctionChoiceBehaviorOptions { RetainArgumentTypes = true }) }),
    // This setting must be set to true when using the ContextualFunctionProvider
    UseImmutableKernel = true
};
#pragma warning restore SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

// Create the agent thread and register the contextual function provider
ChatHistoryAgentThread agentThread = new();

#pragma warning disable SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
agentThread.AIContextProviders.Add(
    new ContextualFunctionProvider(
        vectorStore: new InMemoryVectorStore(new InMemoryVectorStoreOptions() { EmbeddingGenerator = embeddingGenerator }),
        vectorDimensions: 1536,
        functions: GetAvailableFunctions(),
        maxNumberOfFunctions: 3 // Only the top 3 relevant functions are advertised
                                //loggerFactory: LoggerFactory
    )
);
#pragma warning restore SKEXP0130 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

// Invoke the agent
ChatMessageContent message = await agent.InvokeAsync("Get and summarize customer review.", agentThread).FirstAsync();
Console.WriteLine(message.Content);

// Output
//Customer Reviews:
//    -----------------
//    1.John D. - ★★★★★
//       Comment: Great product and fast shipping!
//       Date: 2023 - 10 - 01

//    Summary:
//--------
//The reviews indicate high customer satisfaction,
//highlighting product quality and shipping speed.

//    Available functions:
//    --------------------
//    -Tools - GetCustomerReviews
//    - Tools - Summarize
//    - Tools - CollectSentiments
IReadOnlyList<AIFunction> GetAvailableFunctions()
{
    // Only a few functions are directly related to the prompt; the majority are unrelated to demonstrate the benefits of contextual filtering.
    return new List<AIFunction>
    {
        // Relevant functions
        AIFunctionFactory.Create(() => "[ { 'reviewer': 'John D.', 'date': '2023-10-01', 'rating': 5, 'comment': 'Great product and fast shipping!' } ]", "GetCustomerReviews"),
        AIFunctionFactory.Create((string text) => "Summary generated based on input data: key points include customer satisfaction.", "Summarize"),
        AIFunctionFactory.Create((string text) => "The collected sentiment is mostly positive.", "CollectSentiments"),

        // Irrelevant functions
        AIFunctionFactory.Create(() => "Current weather is sunny.", "GetWeather"),
        AIFunctionFactory.Create(() => "Email sent.", "SendEmail"),
        AIFunctionFactory.Create(() => "The current stock price is $123.45.", "GetStockPrice"),
        AIFunctionFactory.Create(() => "The time is 12:00 PM.", "GetCurrentTime")
    };
}*/
#endregion




// 1) Set up the Azure OpenAI client
//var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ??
//    throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
//var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o-mini";
//var client = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
//    .GetChatClient(deploymentName)
//    .AsIChatClient();


//// 2) Helper method to create translation agents
//static ChatClientAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
//    new(chatClient,
//        $"You are a translation assistant who only responds in {targetLanguage}. Respond to any " +
//        $"input by outputting the name of the input language and then translating the input to {targetLanguage}.");
//// Create translation agents for sequential processing
//var translationAgents = (from lang in (string[])["French", "Spanish", "English"]
//                         select GetTranslationAgent(lang, client));

////// 3) Build sequential workflow
//var workflow = AgentWorkflowBuilder.BuildSequential(translationAgents);


////// 4) Run the workflow
//var messages = new List<ChatMessage> { new(ChatRole.User, "Hello, world!") };

//var run = await InProcessExecution.RunAsync(workflow,messages);
////StreamingRun run = await InProcessExecution.StreamAsync(workflow, messages);
////await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

//foreach (var evt in run.OutgoingEvents)
//{
//    switch (evt)
//    {
//        case AgentResponseUpdateEvent e:
//            Console.WriteLine($"{e.ExecutorId}: {e.Data}");
//            break;

//        case WorkflowOutputEvent output:
//            var result = (List<ChatMessage>)output.Data!;
//            Console.WriteLine("=== FINAL OUTPUT ===");
//            foreach (var msg in result)
//            {
//                Console.WriteLine($"{msg.Role}: {msg.Contents}");
//            }
//            break;
//    }
//}

//List<ChatMessage> result = new();
//await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
//{
//    if (evt is AgentResponseUpdateEvent e)
//    {
//        Console.WriteLine($"{e.ExecutorId}: {e.Data}");
//    }
//    else if (evt is WorkflowOutputEvent outputEvt)
//    {
//        result = (List<ChatMessage>)outputEvt.Data!;
//        break;
//    }
//}

//// Display final result
//foreach (var message in result)
//{
//    Console.WriteLine($"{message.Role}: {message.Contents}");
//}







//PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("https://dockerplan.openai.azure.com/", new AzureCliCredential());

//// 1. Define an agent on the Azure AI agent service
//PersistentAgent definition = await client.Administration.CreateAgentAsync(
//    "<name of the the model used by the agent>",
//    name: "SK-Assistant",
//    description: "You are a helpful assistant.",
//    instructions: "<agent instructions>");

//// 2. Create a Semantic Kernel agent based on the agent definition
//AzureAIAgent agent = new(definition, client);

//AzureAIAgentThread agentThread = new(agent.Client);
//try
//{
//    ChatMessageContent message = new(AuthorRole.User, "<your user input>");
//    await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
//    //await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread)) // 流式响应
//    {
//        Console.WriteLine(response.Content);
//    }

//}
//finally
//{
//    await agentThread.DeleteAsync();
//    await client.Administration.DeleteAgentAsync(agent.Id);
//}


//PersistentAgent agent2 = client.Administration.CreateAgent(
//    model: "",
//    name: "My Friendly Test Agent",
//    instructions: "You politely help with math questions. Use the code interpreter tool when asked to visualize numbers.",
//    tools: [new CodeInterpreterToolDefinition()]
//);
//PersistentAgentFileInfo uploadedAgentFile = client.Files.UploadFile(
//    filePath: "sample_file_for_upload.txt",
//    purpose: PersistentAgentFilePurpose.Agents);
//var fileId = uploadedAgentFile.Id;

//var attachment = new MessageAttachment(
//    fileId: fileId,
//    tools: [new CodeInterpreterToolDefinition()]
//);

//// attach the file to the message
//PersistentThreadMessage message1 = client.Messages.CreateMessage(
//    threadId: new Guid().ToString(),
//    role: MessageRole.User,
//    content: "Can you give me the documented information in this file?",
//    attachments: [attachment]
//);



#region Enhance your agent with custom tools (plugins) and structured output
// https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-agents-are-now-generally-available/

//var builder = Kernel.CreateBuilder();
//builder.AddAzureOpenAIChatCompletion(
//                Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
//                Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
//                Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
//                );
//var kernel = builder.Build();

//kernel.Plugins.Add(KernelPluginFactory.CreateFromType<MenuPlugin>());

//ChatCompletionAgent agent =
//    new()
//{
//Name = "SK-Assistant",
//Instructions = "You are a helpful assistant.",
//Kernel = kernel,
//Arguments = new KernelArguments(new PromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
//};

//ChatMessageContent message = new(AuthorRole.User, "What is the price of the soup special?");

//await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(message))
//{
//Console.WriteLine(response.Message);
//// The price of the Clam Chowder, which is the soup special, is $9.99.
//}
#endregion


sealed class MenuPlugin
{
    [KernelFunction, Description("Provides a list of specials from the menu.")]
    public string GetSpecials() =>
        """
        Special Soup: Clam Chowder
        Special Salad: Cobb Salad
        Special Drink: Chai Tea
        """;

    [KernelFunction, Description("Provides the price of the requested menu item.")]
    public string GetItemPrice(
        [Description("The name of the menu item.")]
        string menuItem) =>
        "$9.99";
}


#pragma warning restore SKEXP0110 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
