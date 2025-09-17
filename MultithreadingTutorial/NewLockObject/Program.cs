using Nito.AsyncEx;
using System.Diagnostics;

var d1 = new Demo();
var d2 = new Demo();

var sw = Stopwatch.StartNew();

var task1 = Task.Run(d1.Foo);
var task2 = Task.Run(d2.Foo);

await Task.WhenAll(task1, task2);

sw.Stop();
Console.WriteLine(sw.ElapsedMilliseconds);


class Demo
{
    // 字符串是有点特殊的引用类型，字面量定义的字符串会被CLR加入到字符串驻留池（Intern Pool），所以 d1 和 d2 中的 _lock 对象实际指向的是同一内存地址，也即是同一个对象
    //private readonly string _lock = "";
    //private readonly object _lock = new {};// 匿名类型 new {}
    private readonly Lock _lock = new ();// System.Threading.Lock C# 9.0 新增锁对象

    // 如果想要在 await 方法中实现锁，可以使用 Nito.AsyncEx nuget 包
    private readonly AsyncLock _asyncLock = new();
    // 如果不想额外引入程序包可以使用官方提供的 SemaphoreSlim 在异步编程中的线程安全，使其成为同步方法
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(1,1);

    public void Foo()
    {
        // 注意：在锁中无法使用 await 方法，因为异步方法无法保证运行前后在同一个线程上
        lock (_lock)
        {
            Thread.Sleep(3000);
            Console.WriteLine("Done");
        }
    }

    public async void FooAsync()
    {
        using (await _asyncLock.LockAsync())
        {
            await Task.Delay(1000);
        }
    }

    public async void FooSemaphoreSlimAsync()
    {
        _signal.WaitAsync();
        await Task.Delay (1000);
        _signal.Release();
    }
}