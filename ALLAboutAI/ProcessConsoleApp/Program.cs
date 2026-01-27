using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Process.Tools;

namespace ProcessConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        // 配置日志  
        using var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // 创建 Kernel  
        Kernel kernel = Kernel.CreateBuilder()
                        .AddAzureOpenAIChatCompletion(
                            Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT"),
                            Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"),
                            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
                        .Build();

        // 创建并运行流程  
        await RunChatBotProcessAsync(kernel);
    }

    static async Task RunChatBotProcessAsync(Kernel kernel)
    {
        // 创建流程构建器  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        ProcessBuilder process = new("ChatBot");
        var introStep = process.AddStepFromType<IntroStep>();
        var userInputStep = process.AddStepFromType<ChatUserInputStep>();
        var responseStep = process.AddStepFromType<ChatBotResponseStep>();
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

        // 定义流程事件路由  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        process
            .OnInputEvent(ChatBotEvents.StartProcess)
            .SendEventTo(new ProcessFunctionTargetBuilder(introStep));

        introStep
            .OnFunctionResult()
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        userInputStep
            .OnEvent(ChatBotEvents.Exit)
            .StopProcess();

        userInputStep
            .OnEvent(CommonEvents.UserInputReceived)
            .SendEventTo(new ProcessFunctionTargetBuilder(responseStep, parameterName: "userMessage"));

        responseStep
            .OnEvent(ChatBotEvents.AssistantResponseGenerated)
            .SendEventTo(new ProcessFunctionTargetBuilder(userInputStep));

        // 构建并启动流程  
        KernelProcess kernelProcess = process.Build();

        await using var runningProcess = await kernelProcess.StartAsync(
            kernel,
            new KernelProcessEvent()
            {
                Id = ChatBotEvents.StartProcess,
                Data = null
            });
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

    }
}