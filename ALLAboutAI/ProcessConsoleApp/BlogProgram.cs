#pragma warning disable SKEXP0080  
// 步骤3  
using Microsoft.SemanticKernel;

namespace ProcessConsoleApp;

/// <summary>
/// https://www.hubwiz.com/blog/semantic-kernel-process-framework-guide/
/// </summary>
public class BlogProgram
{
    static async Task Main1(string[] args)
    {

        #pragma warning disable SKEXP0080

        ProcessBuilder processBuilder = new("BlogGenerationProcess");

        // Add the steps
        var blogGeneratorStep = processBuilder.AddStepFromType<BlogGeneratorStep>();
        var blogResearchStep = processBuilder.AddStepFromType<BlogResearchStep>();
        var blogPublisherStep = processBuilder.AddStepFromType<BlogPublisherStep>();

        // Orchestrate the events
        processBuilder
            .OnInputEvent("Start")
            .SendEventTo(new(blogResearchStep));

        blogResearchStep
            .OnEvent("ResearchDone")
            //.SendEventTo(new(blogGeneratorStep, parameterName: "research"));
            .SendEventTo(new ProcessFunctionTargetBuilder(blogGeneratorStep, parameterName:"research"));

        blogGeneratorStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(blogPublisherStep));

        #pragma warning disable SKEXP0080

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion("gpt-5.1", "https://dockerplan.openai.azure.com/", Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
            .Build();

        // Build and run the process
        KernelProcess process = processBuilder.Build();

        await process.StartAsync(kernel, new KernelProcessEvent { Id = "Start", Data = "Azure Help API" });
    }
}

public class BlogPublisherStep : KernelProcessStep
{
    [KernelFunction]
    public void PublishBlog(string blogContent)
    {
        Console.WriteLine("进入博客发布步骤");

        // 在这里编写发布博客的逻辑  
        // 注意，这里我们不需要Kernel对象，因为我们没有进行任何LLM调用  
        Console.WriteLine(blogContent);
    }
}

// 步骤2  
public class BlogGeneratorStep : KernelProcessStep
{
    [KernelFunction]
    public async Task<string> GenerateBlogContent(string research, Kernel kernel)
    {
        Console.WriteLine("进入博客生成步骤");

        Console.WriteLine($"根据研究生成有见地的博客文章：{research}。");
        var answer = await kernel.InvokePromptAsync($"根据研究生成有见地的博客文章：{research}。");
        return answer.ToString();
    }
}

// 步骤1  
public class BlogResearchStep : KernelProcessStep
{
    [KernelFunction]
    public async Task ResearchBlogTopic(string blogTopic, KernelProcessStepContext context)
    {
        Console.WriteLine("进入博客研究步骤");

        string research = "Microsoft致力于帮助所有客户实现更多目标，Microsoft Azure也不例外。借助Azure，我们希望帮助您推动无限创新。我们的自助支持服务为您提供新的互动方式，并将Azure资源的力量置于您手中。帮助API为您提供访问丰富且强大的自助Azure解决方案的权限，使您能够从首选界面解决Azure问题，而无需创建支持工单。通过使用帮助API，您可以获得诊断见解、故障排除器以及其他针对您的Azure资源和订阅的强大解决方案。这些解决方案由Azure工程师精心策划，旨在加速您在账单、订阅管理和技术问题方面的故障排除体验。本参考文档旨在指导您如何使用帮助API以及每个REST操作的具体参考信息。角色前提条件：要使用帮助API，您必须使用AAD令牌。您必须具有订阅范围的访问权限。对于诊断，您必须对执行诊断的资源具有读取访问权限。解决方案发现和执行步骤：发现解决方案API是发现与订阅或资源映射的解决方案元数据的初始入口点。一旦根据解决方案发现响应中的描述字段确定了解决方案/诊断，您可以使用诊断或解决方案API执行该解决方案。操作组描述 操作 列出所有可用的帮助API REST操作 发现解决方案 使用问题分类ID和资源URI或资源类型列出相关的Azure诊断和解决方案 诊断 为Azure资源创建和列出诊断 解决方案 为选定的订阅或资源创建、更新和列出Azure解决方案 在此版本中，我们将支持技术问题、账单/订阅管理问题（非租户） 故障排除器 作为特定订阅和资源范围的解决方案的一部分创建和列出故障排除器 法律披露：此API连接到Azure服务。您使用此API和它连接的Azure服务受您获得Azure服务时协议的约束。有关更多信息，请参阅Microsoft Azure法律信息 | Microsoft Azure。 其他语言和支持接口 除了REST API支持外，自助API还支持以下接口和语言： Azure Java SDK：工件 | 文档 Azure .NET SDK：工件 | 文档 Azure Python SDK：工件 | 文档 Azure JavaScript SDK：工件 | 文档 Azure Go SDK：工件 | 文档 Azure CLI：代码 | 文档 PowerShell：代码 | 文档 此API连接到Azure服务。您使用此API和它连接的Azure服务受您获得Azure服务时协议的约束。有关更多信息，请参阅Microsoft Azure法律信息 | Microsoft Azure。";
        await context.EmitEventAsync("ResearchDone", research);
    }
}