## 异步方法（async Task）

将方法标记 async 后，可以在方法中使用 await 关键字

await 关键字会等待异步任务的结束，并获得结果

async + await 会将方法包装成状态机，await 类似于检查点

源码：

```C#
using System;
using System.Threading.Tasks;

class Program
{
    public async Task Foo()
    {
        await Task.Delay(1000);
        Console.WriteLine();
    }
}
```

编译后的 C# 代码：

```c#
internal class Program
{
    [StructLayout(LayoutKind.Auto)]
    [CompilerGenerated]
    private struct <Foo>d__0 : IAsyncStateMachine
    {
        public int <>1__state;

        public AsyncTaskMethodBuilder <>t__builder;

        private TaskAwaiter <>u__1;
		
        // MoveNext 方法会被底层调用，从而切换状态
        private void MoveNext()
        {
            int num = <>1__state;
            try
            {
                TaskAwaiter awaiter;
                if (num != 0)
                {
                    awaiter = Task.Delay(1000).GetAwaiter();
                    if (!awaiter.IsCompleted)
                    {
                        num = (<>1__state = 0);
                        <>u__1 = awaiter;
                        <>t__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
                        return;
                    }
                }
                else
                {
                    awaiter = <>u__1;
                    <>u__1 = default(TaskAwaiter);
                    num = (<>1__state = -1);
                }
                awaiter.GetResult();
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                <>1__state = -2;
                <>t__builder.SetException(exception);
                return;
            }
            <>1__state = -2;
            <>t__builder.SetResult();
        }

        void IAsyncStateMachine.MoveNext()
        {
            //ILSpy generated this explicit interface implementation from .override directive in MoveNext
            this.MoveNext();
        }

        [DebuggerHidden]
        private void SetStateMachine([Nullable(1)] IAsyncStateMachine stateMachine)
        {
            <>t__builder.SetStateMachine(stateMachine);
        }

        void IAsyncStateMachine.SetStateMachine([Nullable(1)] IAsyncStateMachine stateMachine)
        {
            //ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
            this.SetStateMachine(stateMachine);
        }
    }

    [NullableContext(1)]
    [AsyncStateMachine(typeof(<Foo>d__0))]
    public Task Foo()
    {
        <Foo>d__0 stateMachine = default(<Foo>d__0);
        stateMachine.<>t__builder = AsyncTaskMethodBuilder.Create();
        stateMachine.<>1__state = -1;
        stateMachine.<>t__builder.Start(ref stateMachine);
        return stateMachine.<>t__builder.Task;
    }
}
```



### 1. async Task

返回值依旧是 Task 类型，但是在其中可以使用 await 关键字

在其中写返回值可以直接写 Task<T> 中的 T 类型，不用包装成 Task<T>

```C#
interface IFoo
{
    Task FooAsync();
}

class Demo : IFoo
{
    public Task FooAsync()
    {
        return Task.FromResult("1");
    }
    
    public Task Foo2Async()
    {
        return Task.Run(() => Console.WriteLine("1"));
    }
    
    public async Task Foo3Async()
    {
        // 可以使用 await 关键字了
        await Task.Delay(1000);
        // 并且不同手动返回 Task 对象， async + await 语法糖会自动返回 Task 对象
    }
    
    public async Task<int> Foo4Async()
    {
        // 可以使用 await 关键字了
        await Task.Delay(1000);
        // 不需要使用 Task.FromResult 包装，而是可以直接返回 Task<TResult> 中的 TResult 类型
        return 43;
    }
    
    public Task<int> Foo5Async()
    {
        Console.WriteLine("Hello");
        return Task.FromResult(21);
    }
    
}
```



### 2. async void

同样是状态机，但缺少记录状态的 Task 对象，因此无法被等待，也无法获取其结果

无法聚合异常（Aggregate Exception），需要谨慎处理异常，无法获取 Task 对象会将异常层层包装往上抛出的特性

几乎只用于对于事件的注册（常用于 WPF 程序中）

源码：

```c#
using System;
using System.Threading.Tasks;

class Program
{
    public async void Foo()
    {
        await Task.Delay(1000);
        Console.WriteLine();
    }
}
```

编译后的 C# 代码的改动：

- public Async**Task**MethodBuilder <>t__builder; 

  变为：public Async**Void**MethodBuilder <>t__builder;

- public **Task** Foo()

  变为：public **void** Foo()



### 3. 代码示例

```C#
using System;
using System.Threading.Tasks;

async Task Main()
{
    try
    {
        // 无法被 await 因为返回值是 void
        // 无法 GetResult 因为返回值是 void
        // VoidAsync();// error CS4008: Cannot await 'void'
        await TaskAsync();
        /*
        System.Exception: Something was wrong!
           at Demo.TaskAsync()
           at Demo.Main()
        */
        // await 关键字语法糖会非常聪明的将 Task 对象层层包裹的 AggregateException 异常拆包，获取里面的 InnerException 真正实际抛出的异常
        // 但是如果我们使用 TaskAsync().Wait() 方法进行调用，这个方法不仅会阻塞异步任务，还无法拆包，而是直接抛出 AggregateException 异常
        /*
        System.AggregateException: One or more errors occurred. (Something was wrong!)
         ---> System.Exception: Something was wrong!
           at Demo.TaskAsync()
           --- End of inner exception stack trace ---
           at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions)
           at System.Threading.Tasks.Task.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
           at Demo.Main()
        */
    }
    // 无法捕捉 async void 方法中的异常，因为没有对应的 Task 对象
    // 程序并不会按照预想中的打印异常信息，而是在 VoidAsycn 方法中抛出了异常
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}

async void VoidAsync()
{
    await Task.Delay(1000);
    throw new Exception("Something was wrong!");
}

async Task TaskAsync()
{
    await Task.Delay(1000);
    throw new Exception("Something was wrong!");
}
```

