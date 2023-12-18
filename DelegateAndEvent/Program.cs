// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

var rule = new MyRule<int>(MySpecialRule);
Console.WriteLine($"{rule?.Invoke(10,20)}");

// 匿名委托：lambda 的前身
Console.WriteLine($"{PickOne(10, 20, delegate (int a, int b) { return a > b;})}");

// 在C#10.0以后可以使用 var 来申明 lambda 表达式编译器会自动推断表达式的委托类型
var f = () => Console.WriteLine($"hello");
f?.Invoke();

var demo = new Demo();
// 注册事件
demo.MyEvent += () => Console.WriteLine("hello");
demo.InvokeEvent();

demo.MyValueChanged += () => Console.WriteLine("value chang observed");
demo.MyValue = 100;// 使用 demo.MyValue = 就是在调用属性 MyValue 的 set 方法
demo.MyValue = 200;
demo.MyValue = 300;

var del = new MyDelegate(F1);
del += F2;// 委托可以“指向”多个函数
del += F22;// 调用委托时，如果其中的一个委托报错，则后面的方法不会被调用
// for (int i = 0; i < 10_000_000; i++)
// {
//     del += F22; // 委托可以指向同一个函数多次
// }
// var sw = Stopwatch.StartNew();
// del += F3;
// // del -= F3;
// del -= F1; // 移除委托是从后往前移：所以移除前面的委托比移除后面的委托更耗时
// sw.Stop();
// Console.WriteLine($"{sw.ElapsedMilliseconds}");


// try
// {
//     del?.Invoke();
// }
// catch (Exception e)
// {
//     Console.WriteLine($"{e}");
// }

#region Covariance(协变) & Contravariance(逆变)
// Covariance：委托可以指向派生类
// Covariance(协变) :将派生类转换为父类,常可以使用隐式转换,常见于 out:要求返回父类类型，但是实际返回的是派生类
// Contravariance(逆变): 将父类转换为其派生类,常需要显式转换,常见于 in:要求接受派生类类型参数，但实际参数传入父类类型
var myObjectDelegate = new MyObjectDelegate(() => "hello");
// 要求返回 object 但是实际返回 string：发生了协变

var myDelegate2 = new MyDelegate2(Foo);
// 需要接受 Animal 类型的参数但是实际上传入了 Dog 类型的参数：发生了逆变

#endregion

demo.NotifyEvent += () => Console.WriteLine("Method 1");
demo.NotifyEvent += () => Console.WriteLine("Method 2");
demo.NotifyEvent -= () => Console.WriteLine("Method 3");
demo.InvokeNotifyEvent();

demo.DemoEvent += () => Console.WriteLine("Method 1");
demo.DemoEvent += () => Console.WriteLine("Method 2");
demo.DemoEvent += () => Console.WriteLine("Method 3");
demo.InvokeDemoEvent();

/* 
特殊且非常不常见的自带委托类型：
只能接受一个参数，并且返回值一定是 bool 类型
当我们编写一个只有一个参数且返回值是 bool 类型
的 lambda 表达式时会被编译成 Func<T1 in, T2 Result>
而不会编译为 Predicate
*/
Predicate<int> predicate = x => x > 10;

void Foo(Animal obj) => Console.WriteLine($"{obj}");

int PickOne(int a, int b, Func<int, int, bool> rule)
{
    if (rule(a, b))
    {
        return a;
    }
    else
    {
        return b;
    }
}

bool MySpecialRule(int x, int y)
{
    return x > y;
}

void F1() => Console.WriteLine($"{nameof(F1)}");
void F2() => Console.WriteLine($"{nameof(F2)}");
void F22() => throw new Exception("Something error");
void F3() => Console.WriteLine($"{nameof(F3)}");


// 通过情况下并不需要我们自定义委托
// 可以使用定义好的 Action 和 Func 委托
delegate bool MyRule<T>(T x, T y);
delegate void MyDelegate();
delegate object MyObjectDelegate();
delegate void MyDelegate2(Dog param);

public class Animal
{
    
}

public class Dog : Animal
{
    
}

public class Demo
{
    private List<Action> _actions = new ();
    // 自定义事件的实现
    public event Action? NotifyEvent
    {
        add
        {
            _actions.Add(value);
        }
        remove
        {
            _actions.Remove(value);
        }
    }

    public void InvokeNotifyEvent()
    {
        foreach (var action in _actions)
        {
            action?.Invoke();
        }
    }

    private int _counter = 0;
    // 也可以重写 add/remove 方法但是完全没有任何意义（不推荐）
    public event Action? DemoEvent
    {
        add => _counter++;
        remove => _counter--;
    }

    public void InvokeDemoEvent()
    {
        Console.WriteLine($"{_counter} method registered");
    }

    int myValue;

    public int MyValue
    {
        get { return myValue; }
        set
        {
            myValue = value;
            // 因为不确定事件是否被注册，所以在调用时加上 ? 表示如果事件没有注册则为 null 则不会调用 Invoke 方法
            MyValueChanged?.Invoke();
        }
    }

    public event Action? MyValueChanged;
    #region public event Action? MyValueChanged 实际编译成的代码
    /* 
    [CompilerGenerated]
    private Action m_MyValueChanged;

    public event Action MyValueChanged
    {
        [CompilerGenerated]
        add
        {
            Action action = this.MyValueChanged;
            while (true)
            {
                Action action2 = action;
                Action value2 = (Action)Delegate.Combine(action2, value);
                // Interlocked 事件是线程安全的
                action = Interlocked.CompareExchange(ref this.MyValueChanged, value2, action2);
                if ((object)action == action2)
                {
                    break;
                }
            }
        }
        [CompilerGenerated]
        remove
        {
            Action action = this.MyValueChanged;
            while (true)
            {
                Action action2 = action;
                Action value2 = (Action)Delegate.Remove(action2, value);
                action = Interlocked.CompareExchange(ref this.MyValueChanged, value2, action2);
                if ((object)action == action2)
                {
                    break;
                }
            }
        }
    }
    */
    #endregion

    // 事件相当于委托的示例，我们不需要事先定义事件，而是直接使用 event 关键字来声明事件即可
    // 当事件报可以为空的编译警告时，在 event 关键字后面的委托类型上加上 ? 表示这是一个可能为空的事件
    public event Action? MyEvent;

    public void InvokeEvent()
    {
        // 事件只能在类的内部调用
        MyEvent?.Invoke();
    }

}

