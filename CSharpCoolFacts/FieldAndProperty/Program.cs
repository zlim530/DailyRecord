using System.Text.Json;

var person  = new Person()
{ 
    Name = "John"
};
person.Age1 = 10;
person.Age2 = 20;
person.DrinkAlcohoc();

Console.WriteLine(typeof(Person).GetProperty("Age1").Name);
Console.WriteLine(typeof(Person).GetField("Age2").Name);

// 标准库中的很多功能只对属性开放，会默认忽略字段
var json = JsonSerializer.Serialize(person);
Console.WriteLine(json);// {"Age1":10,"Name":"John"}


class Person
{
    // Property
    public int Age1 { get; set; }
    // Field
    public int Age2;
    public string Name { get; set; }
    public bool Flag { get; private set; }// 公访私写：只能在类内部进行设置，类外部只能进行访问
    public bool Flag2 { get; }// 只允许在构造函数中赋值
    public bool Flag3 { get; init; } // 只允许在初始化时赋值一次唯一的值

    private bool CanDrinkAlcohol => Age1 >= 18;

    public Person()
    {
        Flag2 = true;
        Flag3 = false;
    }

    public void DrinkAlcohoc()
    {
        if (!CanDrinkAlcohol)
        {
            Console.WriteLine("You are too young to drink alcohol.");
        }
        else
        {
            Console.WriteLine("Cheers!");
        }
    }
}

class Manager
{
    // 私有字段常常用于辅助类内部成员的作用
    private readonly int _uniqueId;
    private readonly ILogger _logger;
    private bool _isLoaded = false;
    private readonly object _lock = new();

    public Manager(ILogger logger)
    {
        _logger = logger;
    }

    public void Load()
    {
        if (_isLoaded)
        {
            _logger.LogInformation("Date is already loaded.");
            return;
        }
        else
        {
            lock (_lock)
            {
                _logger.LogInformation("Loading data.");
                _isLoaded = true;
            }
        }
    }
}

interface ILogger
{
    void LogInformation(string message);
}