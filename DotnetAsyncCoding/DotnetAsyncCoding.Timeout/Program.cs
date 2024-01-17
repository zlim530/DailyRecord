// See https://aka.ms/new-console-template for more information

#region 线程 Thread 超时机制
/*var thread = new Thread(Foo);
thread.Start();
// 当定时结束 Thread 还没有完成就会返回 false
if (!thread.Join(TimeSpan.FromMicroseconds(2000)))
{
    // 此时我们取消这个线程
    thread.Interrupt();
}*/
#endregion

#region 异步任务的超时机制
var cts = new CancellationTokenSource();
// 方式一：使用 Task.WhenAny() 
//var fooTask = FooAsync(cts.Token);
//var completedTask = await Task.WhenAny(fooTask, Task.Delay(2000));
//if (completedTask != fooTask)
//{
//    cts.Cancel();// 要在异步任务被调用前执行 cts.Cancel() 方法才能实现异步任务的超时取消
//    await fooTask;
//    Console.WriteLine("FooTask Timeout ... ");
//}

try
{
    var res = await FooStringAsync(CancellationToken.None).TimeoutAfter<string>(TimeSpan.FromSeconds(2));
    if (res.Contains("FooString"))
    {
        Console.WriteLine("FooStringAsync Task Success!");
    }
    //await FooAsync(CancellationToken.None).TimeoutAfter(TimeSpan.FromSeconds(2));

    // 方式二：使用 Task.WaitAsync() 
    // 在 .Net6 之后支持了 WaitAsync(Timespan timespan)
    await FooAsync(cts.Token).WaitAsync(TimeSpan.FromSeconds(2));
    Console.WriteLine("Wait Async");

    // 方式三：使用 CancellationTokenSource(Timespan timespan)：不推荐
    //var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
    //var task = FooAsync(cancellationTokenSource.Token);
    //await task;
    //Console.WriteLine("cancellationTokenSource TimeSpan.FromSeconds");
}
catch (TimeoutException)
{
    cts.Cancel();
    //Console.WriteLine("Catch AsyncExtension.TimeoutAfter throw TimeoutException()");
    Console.WriteLine("Catch WaitAsync throw TimeoutException()");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Catch cancellationTokenSource throw OperationCanceledException()");
}
finally
{
    cts.Dispose();
}

#endregion

Console.WriteLine("Done.");

async Task FooAsync(CancellationToken token)
{
    try
    {
        Console.WriteLine("Foo start ... ");
        await Task.Delay(5000, token);
        Console.WriteLine("Foo end ... ");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Foo canceled ... ");
        //throw new OperationCanceledException();
    }
}

async Task<string> FooStringAsync(CancellationToken token)
{
    try
    {
        Console.WriteLine("FooString start ... ");
        await Task.Delay(1000, token);
        Console.WriteLine("FooString end ... ");
        return "End in FooString";
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("FooString canceled ... ");
        return "End in Catch block";
    }
}

void Foo()
{
    try
    {
        Console.WriteLine("Foo start ... ");
        Thread.Sleep(5000);
        Console.WriteLine("Foo end ... ");
    }
    catch (ThreadInterruptedException)
    {
        Console.WriteLine("Foo interrupted ... ");
    }
}

static class AsyncExtension
{
    public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask != task)
        {
            cts.Cancel();
            throw new TimeoutException();
        }

        await task;
    }

    public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        if (completedTask != task)
        {
            cts.Cancel();
            throw new TimeoutException();
        }

        return await task;
    }
}
