using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

// 这行代码注册了额外的编码提供程序，使 .NET 应用程序能够使用传统的代码页编码（Code Page Encodings）。
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

#region 用户机密
/*
也可从其他目录使用机密管理器工具。 使用 --project 选项提供项目文件所在的文件系统路径。 例如：
    dotnet user-secrets set "Movies:ServiceApiKey" "12345" --project "C:\apps\WebApp1\src\WebApp1"

访问机密
若要访问机密，请完成以下步骤：
注册用户机密配置源
通过配置 API 读取机密
注册用户机密配置源
用户机密配置提供程序 会向 .NET 配置 API 注册适当的配置源。
通过 dotnet new 或 Visual Studio 创建的 ASP.NET Core Web 应用会生成以下代码：
    var builder = WebApplication.CreateBuilder(args);
    var movieApiKey = builder.Configuration["Movies:ServiceApiKey"];
    var app = builder.Build();
    app.MapGet("/", () => movieApiKey);
    app.Run();

将机密映射到 POCO
对于聚合相关属性来说，将整个对象文字映射到 POCO（具有属性的简单 .NET 类）很有用。
假设应用的secrets.json文件包含下面两个机密：
    {
      "Movies:ConnectionString": "Server=(localdb)\\mssqllocaldb;Database=Movie-1;Trusted_Connection=True;MultipleActiveResultSets=true",
      "Movies:ServiceApiKey": "12345"
    }
若要将上述机密映射到 POCO，请使用 .NET 配置 API 的对象图绑定功能。 下面的代码绑定到自定义 MovieSettings POCO 并访问 ServiceApiKey 属性值：
    var moviesConfig = 
        Configuration.GetSection("Movies").Get<MovieSettings>();
    _moviesApiKey = moviesConfig.ServiceApiKey;

Movies:ConnectionString 和 Movies:ServiceApiKey 机密映射到 MovieSettings 中的相应属性：
    public class MovieSettings
    {
        public string ConnectionString { get; set; }

        public string ServiceApiKey { get; set; }
    }

用机密替换字符串
以纯文本形式存储密码不太安全。 从不将机密存储在配置文件中，例如 appsettings.json，可能会签入源代码存储库。
例如，存储在其中appsettings.json的数据库连接字符串不应包含密码。 而是将密码存储为机密，并在运行时将密码包含在连接字符串中。 例如：
    dotnet user-secrets set "DbPassword" "`<secret value>`"
将 <secret value> 前面的示例中的占位符替换为密码值。 在对象的SqlConnectionStringBuilder属性上Password设置机密的值，将其作为密码值包含在连接字符串中：
    using System.Data.SqlClient;
    var builder = WebApplication.CreateBuilder(args);
    var conStrBuilder = new SqlConnectionStringBuilder(
            builder.Configuration.GetConnectionString("Movies"));
    conStrBuilder.Password = builder.Configuration["DbPassword"];
    var connection = conStrBuilder.ConnectionString;
    var app = builder.Build();
    app.MapGet("/", () => connection);
    app.Run();

非 Web 应用程序中的用户机密
面向 Microsoft.NET.Sdk.Web 的项目会自动包括对用户机密的支持。 对于面向 Microsoft.NET.Sdk（例如控制台应用程序）的项目，请显式安装配置扩展和用户机密 NuGet 包。
    Microsoft.Extensions.Configuration
    Microsoft.Extensions.Configuration.UserSecrets

安装包后，请初始化项目并使用与 Web 应用相同的方式设置机密。 以下示例显示了一个控制台应用程序，该应用程序检索使用密钥“AppSecret”设置的机密的值：
    using Microsoft.Extensions.Configuration;
    namespace ConsoleApp;
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            Console.WriteLine(config["AppSecret"]);
        }
    }
*/
#endregion
IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

var apiKey = config["AZURE_OPENAI_API_KEY"];
var endpoint = config["AZURE_OPENAI_ENDPOINT"];
var deploymentName = config["AZURE_OPENAI_DEPLOYMENT"];

var completeChatClient = new CompleteChatClient(endpoint, deploymentName, apiKey);

var response = await completeChatClient.GenerateWithFunctionCallingAsync("北京今天的天气适合于穿什么衣服?");
Console.WriteLine(response);

public class CompleteChatClient(string endpoint, string deploymentName, string? apiKey = null)
{
    public async Task<string> GenerateWithFunctionCallingAsync(string input,
        CancellationToken cancellationToken = default)
    {
        // Use Azure OpenAI ChatClient for function calling
        var azureClient = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey!));
        var chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

        List<ChatMessage> messages =
        [
            new ChatMessage(ChatRole.System,
                "You are a helpful assistant that can help users with the given function tools."),
            new ChatMessage(ChatRole.User, input)
        ];
        var options = new ChatOptions
        {
            Tools = [AIFunctionFactory.Create(GetWeatherInfo)]
        };
        using var functionCallingChatClient = new ChatClientBuilder(chatClient)
            .UseFunctionInvocation()
            .Build(); //对IChatClient进行包装，支持函数调用功能
        var response = await functionCallingChatClient.GetResponseAsync(messages, options, cancellationToken);
        return response.Text;
    }

    [Description("Get weather information for the specified city")]
    private string GetWeatherInfo([Description("City name, for example: Beijing, Shanghai")] string city)
    {
        // Simulate weather API call - in real scenario, this would call actual weather service
        var weatherData = new
        {
            city,
            temperature = "22°C",
            condition = "Sunny",
            humidity = "65%",
            windSpeed = "10 km/h"
        };

        return JsonSerializer.Serialize(weatherData);
    }
}