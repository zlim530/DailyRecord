namespace AsyncProgramming.CallAsyncFromSync;

class Program
{
    /// <summary>
    /// GetWaiter()/GetResult()
    /// </summary>
    /// <param name="args"></param>
    static void Main01(string[] args)
    {
        try
        {
            #region 阻塞方式
            Console.WriteLine("Begin");
            //使用 Wait() 方法或 Result 属性都是阻塞的
            //FooAsync().Wait();
            //FooExceptionAsync().Wait();// 抛出 System.AggregateException:“One or more errors occurred. (Error!)”
            //由于异步方法可能存在异步方法嵌套异步方法调用的形式，所以每个异步任务的异常也要通过一种机制仿佛向上冒泡一样一层一层往上传递
            //而每往上传递一层都使用的是 AggregateException 对象，它其中有一个 InnerException 会包装内部的异常内容
            //如果使用 GetWaiter() 或者 GetResult() 就不会抛出 AggregateException 异常，而是会自动拆包，抛出 FooExceptionAsync 方法中定义的 Exception 
            //这是 GetWaiter()/GetResult() 与 Wait()/Result 的唯一区别，如果一定要在同步方法中通过阻塞的方式调用异步方法
            //推荐使用 GetWaiter()/GetResult()
            FooExceptionAsync().GetAwaiter().GetResult();
            //如果异步方法没有返回值则 GetResult() 也没有返回值
            //FooAsync().GetAwaiter().GetResult();
            var message = GetMessageAsync().Result;
            Console.WriteLine(message);
            #endregion
            Console.WriteLine("Main Done.");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }


    /// <summary>
    /// 一发即忘（Fire-and-forget）
    /// </summary>
    /// <param name="args"></param>
    static void Main02(string[] args)
    {
        try
        {
            Console.WriteLine("Begin");
            #region 一发即忘（Fire-and-forget）
            // 调用一个异步方法，但是并不使用 await 或阻塞的方式去等待它的结束
            // 无法观察任务的状态（是否完成、是否报错等）
            // FooAsync 中有异常抛出但是无法得知也无法捕获进行处理
            //_ = FooAsync();
            // VoidFooException 抛异常程序会直接挂掉
            VoidFooException();
            #endregion
            Console.WriteLine("Main Done.");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /*
     *"疑难杂点"
     * 1. AsyncLazy: Nito.AsyncEx, Microsoft.VisualStudio.Threading
        Nito.AsyncEx：https://github.com/StephenCleary/AsyncEx
        Microsoft.VisualStudio.Threading：https://github.com/Microsoft/vs-threading
     * 2. JoinableTaskFactory: Microsoft.VisualStudio.Threading
     * 3. Ioc Container: Microsoft.Extensions.DependencyInjection
        如何在 IOC 容易中进行异步方法的调用
     * 4. Unit Test: Microsoft.VisualStudio.TestTools.UnitTesting
        如何对这些异步方法进行单元测试
     *
     * "值得一试"
     * 1. async Task Main
     * 2. async void 
     * 3. AsyncLazy
     * 4. IAsyncEnumerable
     * 5. IAsyncDisposable
     * 6. AsyncRelayCommand
     */
    static void Main(string[] args)
    {
        try
        {
            var dataModel = new MyDateModel();
            Console.WriteLine("Loading data ... ");
            Thread.Sleep(2000);
            var data = dataModel.Data;
            Console.WriteLine($"Is data loaded: {dataModel.IsDataLoaded}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }


    #region 异步方法

    static async Task FooAsync()
    { 
        await Task.Delay(1000);
        GetRandomResult();
        Console.WriteLine("FooAsync Done.");
    }

    static async Task FooExceptionAsync()
    { 
        await Task.Delay(1000);
        throw new Exception("Error!");
    }

    static async Task<string> GetMessageAsync()
    {
        await Task.Delay(1000);
        return "Hello,World!";
    }

    static void GetRandomResult() => throw new NotImplementedException();

    /// <summary>
    /// async void 抛异常程序会直接退出
    /// </summary>
    static async void VoidFooException()
    {
        await Task.Delay(1000);
        await Console.Out.WriteLineAsync("VoidFooAsync Done.");
        GetRandomResult();
    }
    #endregion

}


#region Async Call in Ctor
class MyDateModel
{
    public List<int>? Data { get; private set; }

    public bool IsDataLoaded { get; private set; } = false;

    /// <summary>
    /// 方式3：将需要异步调用的 Task 包装成类的一个私有属性
    /// </summary>
    private readonly Task loadDataTask;

    public MyDateModel()
    {
        //SafeFireAndForget(LoadDataAsync(), () => IsDataLoaded = true, e => throw e);

        //LoadDataAsync().Await(() => IsDataLoaded = true, e => throw e);

        //使用原生自带的 ContinueWith 方法：1. 会将传入的委托包装为一个 Task，会增加程序性能开销；2. 在 TaskScheduler.Current 中运行而不是在 TaskScheduler.Default 中，因此使用 ContinueWith 时都需要专门处理 TaskScheduler 中的一些选项
        //LoadDataAsync().ContinueWith(t => IsDataLoaded = true, TaskContinuationOptions.None);
        LoadDataAsync().ContinueWith(OnDataLoaded);

        //loadDataTask = LoadDataAsync();
    }

    /// <summary>
    /// 暴露 DisplayDataAsync 方法，并在其中对 Task 异步任务进行 await 等待
    /// 不管这个 Task 是否运行完只要有 await 关键字都会被包装成一个状态机，这可能会增大程序的开销
    /// </summary>
    /// <returns></returns>
    public async Task DisplayDataAsync()
    {
        await loadDataTask;
        IsDataLoaded = true;
        Console.WriteLine("Done");
    }

    private bool OnDataLoaded(Task t)
    {
        if (t.IsFaulted)
        {
            // 此时的异常是被包装后的 AggregateException，所以我们可以获取 InnerException 属性拆开
            Console.WriteLine(t.Exception.InnerException?.Message);
            return false;
        }
        else
        {
            return IsDataLoaded = true;
        }
    }

    static async void SafeFireAndForget(Task task, Action? onCompleted = null, Action<Exception>? onFailed = null)
    {
        try
        {
            await task;
            onCompleted?.Invoke();
        }
        catch (Exception e)
        {
            onFailed?.Invoke(e);
        }
    }

    /// <summary>
    /// Mimic loading data from a database 
    /// </summary>
    /// <returns></returns>
    async Task LoadDataAsync()
    {
        await Task.Delay(1000);
        Data = Enumerable.Range(1, 10).ToList();
        //throw new Exception("Failed to load data.");
    }
}
#endregion


#region 使用扩展方法进行 SafeFireAndForget
/// <summary>
/// Brian Lagunas：https://www.youtube.com/watch?v=O1Tx-k4Vao0
/// 使用扩展方法进行 SafeFireAndForget
/// </summary>
static class TaskExtensions
{
    public static async void Await(this Task task, Action? onCompleted = null, Action<Exception>? onFailed = null)
    {
        try
        {
            await task;
            onCompleted?.Invoke();
        }
        catch (Exception e)
        {
            onFailed?.Invoke(e);
        }
    }
}
#endregion


#region Async Factory
class MyService
{
    /// <summary>
    /// 私有化构造函数
    /// </summary>
    private MyService()
    { 
    
    }

    async Task InitService()
    {
        await Task.Delay(1000);
    }

    /// <summary>
    /// 提供 CreateService 工厂方法
    /// </summary>
    /// <returns></returns>
    public static async Task<MyService> CreateService()
    {
        var service = new MyService();
        await service.InitService();
        return service;
    }
}


#endregion
