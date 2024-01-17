// See https://aka.ms/new-console-template for more information

using System.Diagnostics;

// 取消方式二：在 new CancellationTokenSource 传入时间表示过多久后取消：可以写 TimeSpan 也可以直接传入 int
var cts = new CancellationTokenSource(/*TimeSpan.FromSeconds(3)*//*3000*/);
var token = cts.Token;
// 取消方式三：使用 CancelAfter 方法
//cts.CancelAfter(3000);
cts.Cancel();

// 使用 Register 方法进行善后工作：释放资源等
token.Register(() => Console.WriteLine("Cancellation requested in Main Thread"));

var sw = Stopwatch.StartNew();
try
{
    // 可以多次注册 Register 方法，并且后注册的先执行
    token.Register(() => Console.WriteLine("Cancellation requested in Try Block"));
    // 取消方式一：
    //var cancelTask = Task.Run(async () =>
    //{
    //    await Task.Delay(3000);
    //    cts.Cancel();
    //});
    //await Task.WhenAll(Task.Delay(5000, token), cancelTask);
    // 推荐异步方法都带上 CancellationToken 这一传参：最重要的是要将 CancellationToken 参数传入我们想要取消的异步方法中
    //await Task.Delay(5000, token);
    token.Register(() => cts.Dispose());

    // Task.Run 也可以接受 cancellationToken 参数
    // 使用 cts.Cancel(); 抛出 TaskCanceledException 异常：仅在 Task.Run 之前被取消掉才能生效否则一旦 Task.Run 运行后也不会被取消只能作为参数传给其他异步方法
    // 使用 cts.CancelAfter(int millisecondsDelay); 抛出的 exception 是 OperationCanceledException try-catch 无法捕获异常
    await Task.Run(() =>
    {
        for ( int i = 0; i < 10; i++ )
        {
            if (token.IsCancellationRequested)
            {
                token.ThrowIfCancellationRequested();
            }
            Thread.Sleep(1000);
            Console.WriteLine("Pooling ... ");
        }
    }, token);

    Console.WriteLine("Hello,World!");

}
catch (TaskCanceledException e)
{
    Console.WriteLine(e);
}
catch (OperationCanceledException e)
{
    Console.WriteLine(e);
}
finally
{
    cts.Dispose();
}

Console.WriteLine($"Task completed in {sw.ElapsedMilliseconds}ms");

class Demo
{
    // 推荐异步方法都带上 CancellationToken 这一传参
    // 「我可以不用，但你不能没有」：当确实是不需要取消任务时，实现方式有：
    // 1.方法重载
    async Task FooAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(3000, cancellationToken);
    }

    Task FooAsync() => FooAsync(CancellationToken.None);

    // 2.设置默认值 null：当没有传值时，则无法被取消
    // 因为 CancellationToken 类型是一个 struct，因此我们可以使用可为空值类型
    async Task Foo2Async(int delay, CancellationToken? cancellationToken = null)
    {
        var token = cancellationToken ?? CancellationToken.None;
        await Task.Delay(delay, token);
    }

    Task Foo3Async(CancellationToken cancellationToken)
    {
        // 当我们遇到无法使用异步方法的情况时，应该在任务开始前以及每次执行前查看是否需要取消任务
        return Task.Run(() =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                }
                Thread.Sleep(1000);
                Console.WriteLine(" Pooling ... ");
            }
        });
    }
}