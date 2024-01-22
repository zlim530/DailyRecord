using Nito.AsyncEx;

// 1：
var mutex = new AsyncLock();
// Dotnet 原生自带的用于异步编程的信号量 SemaphoreSlim，强大之处在于初始值可以不为1，只不过我们常常写为1来保证只有单线程的处理
// 初始值是1，表示最开始只允许一个线程操作；maxCount 表示最大容量，如果是2则表示最多允许2个线程同时操作
// 不写 maxCount 默认就是 initialCount
// 2：
var semaphore = new SemaphoreSlim(1,1);
// Nito.AsyncEx 扩展包中的
// 3：
var signal = new AsyncAutoResetEvent(false);
// 原生自带的 TaskCompletionSource：是一次性的不可重复使用，有泛型和非泛型版本两种
// 4：
var tcsVoid = new TaskCompletionSource();
var tcs = new TaskCompletionSource<string>();

var start = DateTime.Now;

var tasks = Enumerable.Range(1, 10)
        .Select(x => ComputeAsync(x, mutex))
        .ToList();

//var results = await Task.WhenAll(tasks);
//Console.WriteLine(string.Join(", ", results));

var end = DateTime.Now;
Console.WriteLine($"Elapsed: {(end - start).TotalMilliseconds:F4} ms");

async Task<int> ComputeAsync(int x, AsyncLock mutex)
{
    /*
    如果没有锁那么这10个任务可以同时发生，应该是过300毫秒就结束，
    但是现在加上了锁，每次只允许一个任务（线程）进行操作，
    因此需要3000ms才会结束 
    */
    //await Task.Delay(300);
    //return x * x;

    // 使用 AsyncLock
    //using (await mutex.LockAsync())
    //{
    //    await Task.Delay(300);
    //    return x * x;
    //}

    // 使用 semaphore
    await semaphore.WaitAsync();
    await Task.Delay(300);
    semaphore.Release();
    return x * x;
}

var setter = Task.Run(async () => 
{
    await Task.Delay(2000);
    //signal.Set();
    tcsVoid.SetResult();
    //tcsVoid.SetResult(); // 同一个 TaskCompletionSource 对象不可以多次设置结果，会报错
    tcs.SetResult("Hello");
    if (!tcs.TrySetResult("Hello"))
    {
        Console.WriteLine("Already set!");
    }
    /*
    在 TaskCompletionSource 内部中包装了一个 Task
    通过 TaskCompletionSource 可以设置内部 Task 的状态
    */
});

var waiter = Task.Run(async () => 
{
    // 使用 AsyncAutoResetEvent
    //await signal.WaitAsync();
    // await TaskCompletionSource 内部的 Task
    await tcsVoid.Task;
    var res = await tcs.Task;
    Console.WriteLine(res);
    Console.WriteLine("Signal received!");
});

await Task.WhenAll(setter, waiter);


class Demo
{
    private readonly AsyncLock _lock = new();

    private readonly CancellationTokenSource _tokenSource = new();

    public async Task DoJobAsync()
    {
        // 在异步编程中使用 AsyncLock 进行线程锁的控制
        // 此方法还可以传入 token 进行异步任务的取消
        using (await _lock.LockAsync(_tokenSource.Token))
        {
            // 此外还提供非异步方法的同步 Lock() 方法
            // 这样同步方法和异步方法可以共用一个锁对象
            _lock.Lock();
        }
    }
}