using System.Diagnostics;
using System.Reflection;

namespace ReflectAndAttribute
{
    public class MyBenchmark
    {
        static void Main100(string[] args)
        {
            BenchmarkRunner(typeof(SimpleTester));
            BenchmarkRunnerGeneric<SimpleTester>();
        }
        
        #region 泛型实现
        static void BenchmarkRunnerGeneric<T>(int count = 10_000_000) where T : class, new()
        {
            var obj = new T();

            var methods = typeof(T)
                .GetMethods()
                .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() is not null)
                ;

            Console.WriteLine($"In Generic:");
            foreach (var method in methods)
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    method.Invoke(obj, null);
                }
                Console.WriteLine($"{method.Name} : {sw.ElapsedMilliseconds} ms");
            }
        }
        #endregion

        static void BenchmarkRunner(Type type, int count = 10_000_000)
        {
            var obj = Activator.CreateInstance(type);

            var methods = type
                .GetMethods()
                .Where(m => m.GetCustomAttribute<BenchmarkAttribute>() is not null)
                ;
            
            Console.WriteLine($"In Type");
            foreach (var method in methods)
            {
                var sw = Stopwatch.StartNew();
                for (int i = 0; i < count; i++)
                {
                    method.Invoke(obj, null);
                }
                Console.WriteLine($"{method.Name} : {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}

public class SimpleTester
{
    private IEnumerable<int> testList = Enumerable.Range(1, 10).ToArray();

    [Benchmark]
    public int CalcMinByLINQ()
    {
        return testList.Min();
    }

    [Benchmark]
    public int CalcMinNavie()
    {
        int min = int.MaxValue;
        foreach (var item in testList)
        {
            if (item < min)
            {
                min = item;
            }
        }
        return min;
    }
}

public class BenchmarkAttribute : Attribute
{

}