using System.Runtime.CompilerServices;

namespace DotnetAsyncCoding
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            #region 查询 Task 状态
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
            //Helper.PrintThreadId("Before");
            //await FooAsync();
            //Helper.PrintThreadId("After");
            /*
            1: Main @ Before 1
            2: FooAsync @ Before 1
            3: FooAsync @ After 6
            4: Main @ After 6
            */
            #endregion

            #region 开启多个任务并等待，以及使用信号量
            var inputs = Enumerable.Range(1, 10).ToArray();
            //var tasks = new List<Task<int>>();
            //foreach (var input in inputs)
            //{
            //    tasks.Add(HeavyJobMock(input));
            //}
            // 可以简写为：
            var tasks = inputs.Select(HeavyJobMock).ToList();

            await Task.WhenAll(tasks);// 等待所有 Task 都完成后再通知主线程
            // 当确定 Task 已经完成时，调用 Task.Result 就不会发生同步阻塞
            var outputs = tasks.Select(x => x.Result).ToArray();
            Console.WriteLine(string.Join(",", outputs));
            #endregion
        }

        static async Task<int> HeavyJobMock(int input)
        {
            await Task.Delay(1000);
            return input * input;
        }

        static async Task FooAsync()
        {
            //Thread.Sleep(2000); // 不要在异步方法中使用阻塞方法
            //await Task.Delay(2000);
            Helper.PrintThreadId("Before");
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
