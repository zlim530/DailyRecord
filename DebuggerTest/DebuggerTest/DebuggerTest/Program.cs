// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Hello, World!");

var vm = new Model { Id = 1, Name = "test" , Age = 29};
vm.Foo();

[DebuggerDisplay("{Name}, {Age}")]
class Model
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int Age { get; set; }
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]// 在 debug 时从不显示：保证私密数据的安全性
    public string? Password { get; set; }

    public void Foo()
    {
        var value = PrivateMethod();
    }

    [DebuggerHidden]// debug 时跳过此方法
    private int PrivateMethod()
    {
        var res = 0;
        for (int i = 0; i < 1_000; i++)
        {
            res++;
        }
        return res;
    }
}