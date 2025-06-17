// See https://aka.ms/new-console-template for more information
// Console.WriteLine("Hello, World!");
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using MoreLinq;
using BenchmarkTest;
using CommandLine;

// BenchmarkRunner.Run<ListSortTest>();
// BenchmarkRunner.Run<ListInitTest>();
// BenchmarkRunner.Run<ListReverseTest>();
BenchmarkRunner.Run<LandingPageBenchmarks>();

public class LandingPageBenchmarks
{
    private const string TestInputFile = "C:\\Code\\DailyRecord\\BenchmarkTest\\bin\\Release\\net8.0\\10KTestFile.tsv";
    private const string OutputFileOld = "output_old.txt";
    private const string OutputFileNew = "output_new.txt";
    private Options options = new Options();

    [GlobalSetup]
    public void Setup()
    {
        // Generate test input file with 2000 URLs
        // using (var writer = new StreamWriter(TestInputFile, false, Encoding.UTF8))
        // {
        //     for (int i = 0; i < 2000; i++)
        //     {
        //         writer.WriteLine($"http://example.com/click/{i}\tSomeOtherData\tAdditionalData");
        //     }
        // }
        if (!File.Exists(TestInputFile))
        {
            throw new FileNotFoundException($"Test input file not found: {TestInputFile}");
        }
    }

    [Benchmark]
    public void OriginalCommandProcessor()
    {
        // Simulate original Command.cs processing
        try
        {
            Command.Main(new string[] { TestInputFile, OutputFileOld, "-c", "1000", "-w", "1" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OriginalCommandProcessor: {ex}");
            throw;
        }
    }

    [Benchmark]
    public async Task NewCommandProcessor()
    {
        // Simulate new implementation processing
        await NewProgram.Main(new string[] { TestInputFile, OutputFileNew, "1000", "1" });
    }
}


[MemoryDiagnoser(displayGenColumns:false)]
public class ListSortTest
{
    private List<int> testList;

    [GlobalSetup]
    public void Setup()
    {
        testList = Enumerable.Range(1, 100).Shuffle(new Random(1334)).ToList();
    }

    [Benchmark]
    public List<int> ListSort()
    {
        var list = new List<int>(testList);
        list.Sort();
        return list;
    }

    [Benchmark(Baseline = true)]
    public List<int> ListOrderBy()
    {
        return testList.OrderBy(x => x).ToList();
    }

    [Benchmark]
    public List<int> ListOrder()
    {
        // C# 8.0 语言特性：Indices and Ranges
        // 我们 .. 将运算符称为 范围运算符。 可以大致理解内置范围运算符，使其与此窗体的内置运算符的调用相对
        return [.. testList.Order()];
    }
}


[MemoryDiagnoser(displayGenColumns:false)]
public class ListInitTest
{
    [Params(4, 16, 130)]
    public int count;

    [Benchmark(Baseline = true)]
    public List<int> WithoutInit()
    {
        var res = new List<int>();
        for(int i = 0;i < count;i++)
        {
            res.Add(i);
        }
        return res;
    }

    [Benchmark]
    public List<int> WithInit()
    {
        var res = new List<int>(count);
        for(int i = 0;i < count;i++)
            res.Add(i);
        return res;
    }

    [Benchmark]
    public List<int> WithInitAndRange()
    {
        return Enumerable.Range(0, count).ToList();
    }
}


[MemoryDiagnoser(displayGenColumns:false)]
[Orderer(summaryOrderPolicy:SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class ListReverseTest
{
    private string testString = "hello, world!";

    [Benchmark]
    public string Linq()
    {
        return new string(testString.Reverse().ToArray());
    }

    [Benchmark]
    public string StringBuilder()
    {
        var sb = new StringBuilder();
        for(int i = testString.Length - 1;i >= 0;i--)
        {
            sb.Append(testString[i]);
        }
        return sb.ToString();
    }

    [Benchmark]
    public string CharArray()
    {
        var array = testString.ToCharArray();
        array.Reverse();
        return new string(array);
    }
}
