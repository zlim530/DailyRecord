using System.Runtime.CompilerServices;

namespace DotnetAsyncCoding
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            #region 查询 Task 状态
            // Task 是包含了异步任务的各种状态的一个引用类型
            // 开启异步任务后，当前线程并不会阻塞，而是可以去做其他事情
            // 异步任务（默认）会借助线程池在(其他)线程上运行：异步编程不必需要多线程来实现，单线程同样可以异步（通过 CPU 时间片轮转调度实现）
            // 单线程异步：自己定好计时器，到时间之前先去做别的事情
            // 多线程异步：将任务交给不同的线程，并由自己来进行指挥调度
            // 单线程异步类似于我们在一边烧水的同时一边做其他的事，当水烧开后我们再去处理烧开的水
            // （其他的事不需要等待水烧开后才能去做，那么我们就说烧水操作是异步的）
            // 等 Task 运行结果获取结果后回到之前的状态（通过状态机实现）
            /*var task = new Task<string>(() =>
            {
                Thread.Sleep(1500);
                return "done";
            });

            Console.WriteLine(task.Status);
            task.Start();
            Console.WriteLine(task.Status);
            Thread.Sleep(1000);
            Console.WriteLine(task.Status); 
            Thread.Sleep(2000);
            Console.WriteLine(task.Status);
            Console.WriteLine(task.Result);*/
            /*
            Created
            WaitingToRun
            Running
            RanToCompletion
            done
            */
            #endregion

            #region 查看当前线程的 ID
            Helper.PrintThreadId("Before");
            await FooAsync();
            Helper.PrintThreadId("After");
            /*
            1: Main @ Before 1
            2: FooAsync @ Before 1
            3: FooAsync @ After 6
            4: Main @ After 6
            */
            #endregion

            #region 同时开启多个异步任务并等待，以及使用信号量
            // 如何同时开启多个异步任务？不要 for 循环中使用 await，而是使用 Task.WhenAll()、Task.WhenAny()
            var inputs = Enumerable.Range(1, 10).ToArray();
            // 设定初始窗口为2，最大窗口也为2，表示每次最多有2个线程可以运行：使用信号量来控制异步任务的触发时间
            //var tasks = new List<Task<int>>();
            //foreach (var input in inputs)
            //{
            //    tasks.Add(HeavyJobMock(input));
            //}
            // 可以简写为：
            var tasks = inputs.Select(HeavyJobMock).ToList();

            //await Task.WhenAll(tasks);// 等待所有 Task 都完成后再通知主线程
            //// 当确定 Task 已经完成时，调用 Task.Result 就不会发生同步阻塞
            //var outputs = tasks.Select(x => x.Result).ToArray();
            //Console.WriteLine(string.Join(",", outputs));
            #endregion
        }

        public static SemaphoreSlim sem = new SemaphoreSlim(2, 2);

        static async Task<int> HeavyJobMock(int input)
        {
            await sem.WaitAsync();
            await Task.Delay(1000);
            sem.Release();
            return input * input;
        }

        static async Task FooAsync()
        {
            Helper.PrintThreadId("Before 01");
            //Thread.Sleep(2000); // 不要在异步方法中使用阻塞方法
            await Task.Delay(2000); // await 会暂时释放当前线程，使得该线程可以执行其他工作，而不必阻塞线程直到异步操作完成
            Helper.PrintThreadId("Before 02");
            await Task.Delay(1000);
            Helper.PrintThreadId("After");
        }
    }

    class Helper
    {
        private static int index = 1;
        public static void PrintThreadId(string? message = null, [CallerMemberName] string? name = null)
        {
            var title = $"{index}: {name}";
            if (!string.IsNullOrEmpty(message))
                title += $" @ {message}";
            Console.WriteLine($"{title} {Environment.CurrentManagedThreadId}");
            Interlocked.Increment(ref index);
        }
    }
}
