// See https://aka.ms/new-console-template for more information

#region 线程不安全的两个例子
// 1.示例1:计数
/*const int total = 100_000;

int count = 0;
object lockObj = new();

var thread1 = new Thread(ThreadMethod);
var thread2 = new Thread(ThreadMethod);

thread1.Start();
thread2.Start();

thread1.Join();// 阻塞主线程，让主线程等待子线程的结束
thread2.Join();

Console.WriteLine($"Count: {count}");

void ThreadMethod()
{
    for (int i = 0; i < total; i++)
        // 加上锁相当于使用了同步机制：用于协调和控制多个线程之间执行顺序和互斥访问共享资源确保线程按照特定的顺序执行，避免竞态条件和数据不一致的问题
        //lock (lockObj)
        //count++;// 如果不加锁则是线程不安全的
        // 两个线程会同时访问 count 变量，当 count = 1000，要进行+1操作时线程1进行了+1变成1001，但是线程2无法访问到1001，它访问到的也还是1000，所以再次+1，这样每个线程都做了total次的自增，但是两个线程会多次出现无法访问到 count 最新值的情况，所以 count 的值不是预期中的 20k
        Interlocked.Increment(ref count);// 还可以使用内置的 Interlocked 对象：Increment 实现的效果就是给变量 + 1
}*/

// 2.示例2:操作队列
//var queue = new Queue<int>();
using System.Collections.Concurrent;
// 线程安全的集合类型：ConcurrentBag、ConcurrentStack、ConcurrentQueue、ConcurrentDictionary
// 避免重复造轮子：使用线程安全的 ConcurrentQueue 后就不需要再加锁之类的操作
var queue = new ConcurrentQueue<int>();

var producer = new Thread(Producer);
var consumer1 = new Thread(Consumer);
var consumer2 = new Thread(Consumer);

producer.Start();
consumer1.Start();
consumer2.Start();

producer.Join();
Thread.Sleep(100); // Wait for consumers to finish

consumer1.Interrupt();
consumer2.Interrupt();
consumer1.Join();
consumer2.Join();

void Producer()
{
    for (int i = 0; i < 20; i++)
    {
        Thread.Sleep(20);
        queue.Enqueue(i);
    }
}

void Consumer()
{
    try
    {
        while (true)
        {
            if (queue.TryDequeue(out var res))
                Console.WriteLine(res);
            Thread.Sleep(1);
        }
    }
    catch (ThreadInterruptedException)
    {
        Console.WriteLine("Thread interrupted.");
    }
}

#endregion

#region 线程的创建和终止

/*var th = new Thread((obj) =>
{
    try
    {
        *//*Console.WriteLine(obj);
        for (int i = 0; i < 20; i++)
        {
            Thread.Sleep(100);
            Console.WriteLine("Thread is still running ... ");
        }*//*

        while (true)
        {
            // 什么操作都没有程序无法在子线程中抛出异常无法终止子线程
            // 因此可以写上 Thread.Sleep(0);
            Thread.Sleep(0);
        }
    }
    catch (ThreadInterruptedException) { }
    finally
    {
        Console.WriteLine("Thread is finished!");

    }
   
})
{ IsBackground = true, Priority = ThreadPriority.Normal };// 不进行配置默认就是 normal 优先级

th.Start(123);
Console.WriteLine("In main thread, waiting for thread to finish ... ");
Thread.Sleep(1000);
th.Interrupt();// 中断线程的执行，会在相应线程中抛出 ThreadInterruptedException
// 如果线程中包含一个 while(true) 循环，那么需要保证包含等待方法，如IO操作，Thread.Sleep(0) 等：否则程序将找不到抛出异常的时机
th.Join();
Console.WriteLine("Done.");*/

#endregion

#region 线程安全和同步机制
/*using System.Diagnostics;

var inputs = Enumerable.Range(1, 20).ToArray();

var outputs = new int[inputs.Length];

var sw = Stopwatch.StartNew();
// 锁与信号量:Thread-Safety
var semaphore = new Semaphore(3, 3);
// Sequential：单线程非常耗时
//for (int i = 0; i < inputs.Length; i++)
//{
//    outputs[i] = HeavyJobMock(inputs[i]);
//}

// Parallel：自带多线程方法的实现
//Parallel.For(0, inputs.Length, i => outputs[i] = HeavyJobMock(inputs[i]));

// PLINQ：自带多线程方法的实现
//var plinqOutputs = inputs.AsParallel().Select(i => HeavyJobMock(i)).ToArray();
//outputs = inputs.AsParallel().Select(HeavyJobMock).ToArray();
outputs = [.. inputs.AsParallel().AsOrdered().Select(HeavyJobMock)]; // 使用集合表达式

Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds}ms");

semaphore.Dispose();

PrintArray(outputs);

int HeavyJobMock(int input)
{
    semaphore.WaitOne();
    Thread.Sleep(100);
    semaphore.Release();
    return input * input;
}

void PrintArray<T>(T[] arr)
{
    //var str = "";
    //for (int i = 0; i < arr.Length; i++)
    //{
    //    if (i == arr.Length - 1)
    //    {
    //        str += arr[i];
    //    }
    //    else
    //    {
    //        str = str + arr[i] + ", ";
    //    }
    //}
    //Console.WriteLine(str);
    Console.WriteLine(string.Join(",", arr));
}*/

#endregion