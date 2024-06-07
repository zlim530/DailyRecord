using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

/*
using var host = Host.CreateDefaultBuilder(args)
    .UseEnvironment("Development")
    .ConfigureAppConfiguration((context, builder) =>
    { 
        builder.SetBasePath(context.HostingEnvironment.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args);
    })
    // .ConfigureLogging()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<UserSettings>();
        services.AddSingleton<UserSettings>();
    })
    .Build();

host.Run();

info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\code\DailyRecord\AppSettingsTutorial\AppSettingsTutorial\bin\Debug\net8.0 
*/

var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

var logLevel = configuration["Logging:LogLevel:System"];
var randomValue = configuration["RandomValue"];

//var connectionString = configuration["ConnectionString:Default"];

var connectionString = configuration.GetConnectionString("Default");

Console.WriteLine($"LogLevel: {logLevel}");
Console.WriteLine($"RandomValue: {randomValue ?? "NULL"}");
Console.WriteLine($"ConnectionString: {connectionString}");

var width = configuration["UserSettings:Width"];
Console.WriteLine($"Width: {width}");

class UserSettings
{
    public string? Title { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Flag { get; set; }
}