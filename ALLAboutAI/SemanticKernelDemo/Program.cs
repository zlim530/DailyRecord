//https://www.nuik.cn/semantic-kernel

using Microsoft.SemanticKernel;

var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        deploymentName: "gpt-5.1",
        endpoint: "https://dockerplan.openai.azure.com/",
        apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY"))
    .Build();

var translate = kernel.CreateFunctionFromPrompt(
     "将以下文本翻译成{{$language}}：{{$text}}"
);

var result = await kernel.InvokeAsync(translate, new()
{
    ["text"] = "Hello World",
    ["language"] = "中文"
});

kernel.ImportPluginFromObject(new MathPlugin());

result = await kernel.InvokeAsync("MathPlugin",
    "Add",
    new() { ["a"] = 5, ["b"] = 3 });

Console.WriteLine(result);


public class MathPlugin
{
    [KernelFunction("Add")]
    public int Add(int a, int b)
    {
        return a + b;
    }

    [KernelFunction("GetTime")]
    public string GetTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}