using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
#if DEBUG
Console.WriteLine("DEBUG is defined");
#else
Console.WriteLine("DEBUG is not defined");
#endif

var student = new Student(1, "John", "A", 80);
var json = JsonConvert.SerializeObject(student);
Console.WriteLine(json);

public record Student(int Id, string Name, string Class, int Score);

/* 
# -n 建立名称为 ConsoleApp 控制台程序
$ dotnet new console -n ConsoleApp
# 运行工程 ConsoleApp
$ dotnet run --project ConsoleApp
# 以 release 方式运行 
$ dotnet run -c Release


# 安装 Newtonsoft.Json 版本13.0.0
$ dotnet add package Newtonsoft --version 13.0.0
# 默认安装 Newtonsoft.Json 最新版本
$ dotnet add package Newtonsoft.Json


$ dotnet new xunit -n ConsoleApp.Test
# 添加引用：将 ConsoleApp 项目添加到 ConsoleApp.Test 中
$ dotnet add ConsoleApp.Test reference ConsoleApp
# 运行 ConsoleApp.Test 测试程序
$ dotnet test ConsoleApp.Test


# 建立名称为 ConsoleApp 的解决方案文件
$ dotnet new sln -n ConsoleApp
# 将文件夹下的工程都放入一个解决方案文件
$ dotnet sln add ConsoleApp
$ dotnet sln add ConsoleApp.Test
# 如果解决方案添加了测试项目则可以直接使用 dotnet test 运行测试项目
$ dotnet test


# 在项目文件夹下新建 dotnet 标准 gitignore 文件
$ dotnet new gitignore
*/