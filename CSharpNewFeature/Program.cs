// See https://aka.ms/new-console-template for more information
using System.Runtime.CompilerServices;
using System.Collections;

var arr = new [] {1, 3, 5, 7};
if (arr is [1, not 2, <6, >=6])// 模式匹配：C# 11.0
{
    Console.WriteLine($"Pass");
}

var enums = Enum.GetValues<Colors>().Select(c => (c, c.GetTypeCode(), (int)c));

foreach (var e in enums)   
{
    Console.WriteLine($"{e.c} {e.Item2} {e.Item3}");
}

// await new MyDelay(10_000);
// await 3000;

// foreach (var item in new MyEnumerator())
// foreach (var item in 5)
await foreach (var item in new MyAsyncEnumerator())
{
    Console.WriteLine($"{item}");
}

Console.WriteLine("Hello, World!");

#region GetAwait()
class MyDelay
{
    public int Seconds  { get; private set; }
    public MyDelay(int seconds)
    {
        this.Seconds = seconds;
    }
}

static class MyExtensions
{
    public static TaskAwaiter GetAwaiter(this MyDelay md)
    {
        return Task.Delay(md.Seconds).GetAwaiter();
        // return Task.Delay(TimeSpan.FromSeconds(md.Seconds)).GetAwaiter();
    }

    public static TaskAwaiter GetAwaiter(this TimeSpan ts) => Task.Delay(ts).GetAwaiter();

    public static TaskAwaiter GetAwaiter(this int seconds) => Task.Delay(seconds).GetAwaiter();
}
#endregion


#region GetEnumerator()
class MyEnumerator 
{
    public IEnumerator GetEnumerator()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return i.ToString();
        }
    }
}

class MyAsyncEnumerator : IAsyncEnumerable<string>
{
    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < 5; i++)
        {
            await Task.Delay(200);// 每隔0.2毫秒打印一个数字
            yield return i.ToString();
        }
    }
}

static class MyExtension2s
{
    public static IEnumerator GetEnumerator(this int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return i.ToString();
        }
    }
}

#endregion

enum Colors : byte
{
    Red = 0x01,
    Green,
    Blue
}