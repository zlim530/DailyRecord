using System.Reflection;

namespace ReflectAndAttribute
{
    public class MySerialize
    {
        static void Main010(string[] args)
        {
            var student = new Student()
            {
                Id = 13,
                Name = "Tom",
                Age = 17,
                Gender = Gender.Male,
                Class = "3A"
            };
            var myJson = Serialize(student);
            Console.WriteLine($"{myJson}");
            
            Foo(100, tag: "test");
        }

        // 使用反射实现序列化
        static string Serialize(object obj)
        {
            var res = obj
                .GetType()
                .GetProperties()// 默认不写就是选择下面 public 非静态的属性
                //.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                //.Where(pi => pi.GetCustomAttribute<BrowsableAttribute>() is null)// 简洁写法
                .Where(pi => 
                {
                    var attr = pi.GetCustomAttribute<BrowsableAttribute>();
                    if (attr is not null) return attr.Browsable;
                    return true;
                })
                .Select(pi => new { Key = pi.Name, Value = pi.GetValue(obj)})
                .Select(o => $"{o.Key} : {o.Value}")
                ;
            return string.Join(Environment.NewLine, res);
        }

        // 关于位置参数
        static void Foo(int value, bool flag = false, string tag = ""){}
    }
}

class Student
{
	[Browsable(false, Tag = "Test")]// 使用这种方式可以对特性类构造函数中没有出现的属性进行赋值
    // 这种写法只会在 Attribute 赋值中出现
	public int Id { get; set; }
	public string Name { get; set; }
	public int Age { get; set; }
	public Gender Gender { get; set; }
	public string Class { get; set; }
}

enum Gender { Male, Female };

[AttributeUsage(AttributeTargets.Property)]// 编译器在使用：表示此特性类只能作用于属性上
class BrowsableAttribute : Attribute
{
	public bool Browsable { get; set; }
    public string Tag { get; set; } = "Default";
	
	public BrowsableAttribute(bool b)
	{
		Browsable = b;
	}
}