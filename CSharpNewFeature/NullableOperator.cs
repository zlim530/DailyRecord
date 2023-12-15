namespace CSharpNewFeature
{
    public class NullableOperator
    {
        static void MainNullable(string[] args)
        {
            /* 
            C# 1.0：三目运算符
                    空合并 ??
            C# 2.0：可为空的值类型 int? Nullable<int>
            C# 6.0: 空传播 ?. ?[]
            C# 7.0: ?? throw
            C# 8.0: 空合并赋值 ??=
                    可为空的引用类型 string?
                    空包容 !
            */
            // C# 1.0 三目运算符
            string isInLove = HasCrush() ? "Yes" : "No";

            #region  C# 2.0 可为空的值类型 Nullable Value Types
            // C# 2.0 可为空的值类型 Nullable Value Types：struct Nullable<T> where T : struct
            // Nullable<int> x = null;
            int? x = null;
            if (x.HasValue)
                Console.WriteLine($"{x.Value}");
            int? y = default;
            if (y == null)
                Console.WriteLine($"true");
            
            // 可以用来声明数组，以及作为泛型的类型
            int?[] arr1;// Nullable<int>[]
            int[]? arr2;// int[] 表示可为空的引用类型
            int?[]? arr3;// Nullable<int>[] 
            List<int?> list1;// List<Nullable<int>> 类型是 Nullable<int> 的集合
            List<int?>? list2;// List<Nullable<int>> 类型是 Nullable<int> 其中可能存在 null 值的集合

            // 还可以用来声明交错数组
            var jagged = new int?[][]
            {
                new int?[] {1, 2, 3},
                new int?[] {4, 5},
                // null // 无法将 null 字面量转换为非 null 的引用类型
                // 在 jagged 中声明 null 虽然也不会报错，但是会有警告
            };
            // 这个还是 Nullable reference type，jagged2 和 jagged 类型是一样
            // 只不过 jagged2 交错数组中可能存在 null 值：第二个 ? 就是表示可为空的引用类型：这在 C# 8.0 中出现
            var jagged2 = new int?[]?[]
            {
                new int?[] {1, 2, 3},
                new int?[] {4, 5},
                null
            };
            #endregion

            #region  C# 6.0 Null Propagator（空引用传播）
            // C# 6.0 Null Propagator（空引用传播）
            #nullable disable warnings
            Person p = null;
            // 短路机制
            // 对于属性的访问，遇到第一个 null 就直接返回 null，不会再继续执行后面的代码
            string childName = p?.Child?.Name;
            // 对于方法的调用，遇到第一个 null 就会停止方法链
            p?.Child?.SayHello();
            // ?[]
            var firstLetter = p?.Name?[0];// char? 类型
            #nullable enable warnings
            
            // 在没有 ?. 之前需要人为进行 null 检查
            /*
            if (p != null)
                if(p.Child != null)
                    if(p.Child.Name != null)
                        Console.WriteLine(p.Child.Name);
            
            if (p == null)
                return;
            if (p.Child == null)
                return;
            if (p.Child.Name == null)
                return;
            Console.WriteLine(p.Child.Name);
            */     

            #endregion

            #region  C# 7.0 throw 语法
            // C# 7.0 throw 语法：在 ? 问号运算符后面可以 throw Exception
            int? input = 1;
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            var res = input ?? throw new ArgumentNullException();
            var r = input != null ? 0 : throw new ArgumentNullException();

            int? inpu2t = 2;
            ArgumentException.ThrowIfNullOrEmpty(nameof(inpu2t));
            #endregion

            #region  C# 8.0 Null-Coalescing ??（空合并）??=（空合并赋值）
            // C# 8.0 Null-Coalescing ??（空合并）[C#1.0就有了] ??=（空合并赋值） [C#8.0才有]
            int? a = null;
            Console.WriteLine(a ?? -1);
            Foo(1);
            #endregion

            #region  C# 8.0 Nullable Reference Type 可为空的引用类型
            // C# 8.0 Nullable Reference Type 可为空的引用类型
            var person = new Person();
            var email = person?.Email?.ToLower();

            var repo = new StudentRepo();
            var student = repo.GetStudentById(12);
            // 1. 使用 ?. 短路机制
            // Console.WriteLine($"{student?.Name}"); 
            // 2. 人为 null checking
            // if (student == null)    
            //     throw new Exception();
            ArgumentNullException.ThrowIfNull(student);
            // var studen2t = repo.GetStudentById(1) ?? throw new Exception();
            // 编译器很聪明的发现我们已经在前面做了 null checking 
            // 所以下面打印 Name 字段时就不会再出现空引用错误于是空引用波浪线也消失了
            Console.WriteLine($"{student.Name}");
            #endregion
        
            #region  C# 8.0 Null-Forgiving Operator !（空包容/抑制）
            // C# 8.0 Null-Forgiving Operator !（空包容/抑制）
            var perso2n = new Person() { Name = "Tim"};
            // 这里假如非常确定 perso2n 对象的 Name 属性不为空，那么可以写 ! 提示编译器不要报可能为空错误
            perso2n!.Name = "Hello";
            #endregion
        
        }

        static void Foo(int x, string message = null)
        {
            // message = message ?? GetMessage();
            message ??= GetMessage(); 
            // 如果 message 不等于 null 则就是它本身，如果等于 null 则被赋值为 GetMessage() 的返回值
        }

        static string GetMessage() => "Hello";

        static bool HasCrush()
        {
            return true;
        }
    }
}

public class StudentRepo
{
    private readonly List<Student> _students = new();

    public Student? GetStudentById(int id)
    {
        return _students.FirstOrDefault(s => s.Id == id);
    }
}

public record Student(int Id, string Name);

public class Person
{
    public string Name { get; set; }
    public Person Child { get; set; }
    // 提供了一个在编译期时就可以判断是否有空引用错误的机会
    public string? Email { get; set; } 
    public void SayHello() => Console.WriteLine("Hello!");
}