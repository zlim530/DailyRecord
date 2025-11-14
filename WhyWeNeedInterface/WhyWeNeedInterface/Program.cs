using Microsoft.Extensions.DependencyInjection;
using WhyWeNeedInterface.Services;

//var provider = new SystemTimeProvider();
//var service = new GreetingService(provider);
var container = new ServiceCollection();
container.AddSingleton<ITimeProvider, SystemTimeProvider>();
// sp => new GreetingService(sp.GetRequiredService<ITimeProvider>(), true):工厂方法，对于 bool 这种基础类怎么传入 IOC 容器中
//container.AddSingleton<GreetingService>(sp => new GreetingService(sp.GetRequiredService<ITimeProvider>(), true));
container.AddSingleton<GreetingService>();
var services = container.BuildServiceProvider();
var greetingService = services.GetRequiredService<GreetingService>();

var message = greetingService.GetGreetingMessage("Lim");
Console.WriteLine(message);
