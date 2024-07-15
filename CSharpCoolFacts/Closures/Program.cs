var actions = CreateActions();

/*
Action<int>[] 数组内都是存的完全相同的函数，即接受一个 int 型参数的 Action<int>，也就是这里的变量 x
下面调用委托时，x 的值为10，而 i 是“捕获”的变量，这个变量是可以被外界修改的
*/
actions[0].Invoke(10);
actions[1].Invoke(10);
actions[2].Invoke(10);

Action<int>[] CreateActions(int count = 3)
{
    var actions = new Action<int>[count];
    for (int i = 0; i < count; i++)
    {
        /*
        actions[i] = x => Console.WriteLine(x * i);
        如果直接使用 i，则在调用时会输出3个30，因为闭包是使用时才生效；
        当我们在 Invoke 的时候不是 i 当时的值，而是使用时候的值；
        而当走到委托调用时，循环已经结束，i = 3，所以 Invoke 的时候一直都是 3 了
        新增临时变量 j 保存当时 i 的值即可实现输出"0,10,20"的效果
        */
        int j = i;
        actions[i] = x => Console.WriteLine(x * j);
    }

    return actions;
}

//int count = 0;
//var demo = new Demo(() => count++);
//Console.WriteLine("Before" + count);
//demo.DoJob();
//Console.WriteLine("After" + count);


class Demo
{
    private readonly Action _callback;

    public Demo(Action callback)
    {
        _callback = callback;
    }

    public void DoJob()
    {
        Console.WriteLine("Job done.");
        _callback();
    }
}