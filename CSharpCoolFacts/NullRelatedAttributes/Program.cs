using System.Diagnostics.CodeAnalysis;

// 利用C#近几个版本的新特性来辅助编译器的空引用检查
// https://www.bilibili.com/video/BV1iQskevEib/?vd_source=2dd2bc4ccab440b2097d82f8cd105cea
// 官方文档：https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/attributes/nullable-analysis
// 吕毅的博客：https://blog.walterlv.com/post/csharp-nullable-analysis-attributes.html

Console.WriteLine("Null checking with attributes!");

#region MemberNotNull
/*
var p = new Employee();
var l = p.FirstName.Length;
if (p.InitMiddleName())
    l = p.MiddleName.Length;

class Employee
{
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }

    public Employee()
    {
        // 可以使用 MemberNotNull 特性来简化
        // FirstName = "John";
        // LastName = "Wick";
        Init();
    }

    [MemberNotNull(nameof(FirstName),nameof(LastName))]
    private void Init()
    {
        FirstName = "John";
        LastName = "Wick";
    }

    [MemberNotNullWhen(true, nameof(MiddleName))]
    public bool InitMiddleName()
    {
        if (CheckMiddleNameNull())
        {
            MiddleName = "Zoe";
            return true;
        }

        return false;
    }

    private bool CheckMiddleNameNull()
    {
        return MiddleName != null;
    }

    public void ShowInfo()
    {
        if(MiddleName is not null)
            Console.WriteLine(FirstName.Length + MiddleName.Length + LastName.Length);
    }
}
*/
#endregion

#region AllowNull & DisallowNull

// NotNullWhen
/*
var line = "# Hello, world!";
if (TryGetTitle(line, out var title))
{
    Console.WriteLine($"Title length: {title.Length}");
}
else
{
    Console.WriteLine("No title found!");
}

static bool TryGetTitle(string line, [NotNullWhen(true)]out string? title)
{
    // 解析 Markdown 的一级标题
    if (line.StartsWith("# "))
    {
        title = line.Substring(2);
        return true;
    }

    title = null;
    return false;
}
*/

// AllowNull
/*
var demo = new Demo();
demo.Message = null;// 可以使用 ! 来忽略:null!
Console.WriteLine(demo.Message.Length);

class Demo
{
    private string _message = "";

    [AllowNull]
    public string Message
    {
        get => _message;
        set => _message = value ?? GetRandomMessage();
    }

    private string GetRandomMessage()
    {
        return "Random Message";
    }
}
*/
#endregion

#region NotNullIfNotNull
/*
var cache = new Cache();
var value = cache.GetCache("key");
Console.WriteLine(value.Length);

class Cache
{
    private readonly Dictionary<string, string> _cache = new();

    [return: NotNullIfNotNull(nameof(key))]// 表示当入参 key 不为空时返回值就不会为空
    public string? GetCache(string? key)
    {
        if (key != null)
        {
            return _cache[key];
        }

        return null;
    }
}
*/
#endregion