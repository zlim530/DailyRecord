using System.Linq.Expressions;
using System.Reflection;

namespace ReflectAndAttribute
{
    public class ModifyPropValue
    {
        static void Main(string[] args)
        {
            var demo = new Demo();
            demo.FieldValue = 10;
            demo.PropValue = 10;

            // 修改字段的值
            ModifyFieldValue(ref demo.FieldValue, 42);
            Console.WriteLine($"Field Value: {demo.FieldValue}");
            
            // 修改属性的值
            // ModifyFieldValue(ref demo.PropValue, 42);
            ModifyPropValueWithDelegate(val => demo.PropValue = val, 42);

            var method = typeof(Demo).GetProperty("PropValue")!.GetSetMethod()!;//确定它肯定不为空于是使用! 空抑制符来告诉编译器不要再报可能为空错误
            // ModifyPropValueWithReflection(method, demo, 42);

            // 传入一个表达式然后在实现方法内部通过传入的 getter 来找到这个属性对应 setter 方法
            ModifyPropValueWithExpression(d => d.PropValue, demo, 42);
            Console.WriteLine($"Prop Value: {demo.PropValue}");
        }

        static void ModifyFieldValue<T>(ref T field, T newValue)
        {
            field = newValue;
        }

        static void ModifyPropValueWithDelegate<T>(Action<T> func, T newValue)
        {
            func.Invoke(newValue);
        }

        static void ModifyPropValueWithReflection<TClass, TProp>(MethodInfo method, TClass target, TProp newValue)
        {
            method.Invoke(target, new object[] { newValue });
        }

        static void ModifyPropValueWithExpression<TClass, TProp>(Expression<Func<TClass, TProp>> expression, TClass target, TProp newValue)
        {
            var body = (MemberExpression)expression.Body;
            var prop = (PropertyInfo)body.Member;
            var setMethod = prop.GetSetMethod()!;
            setMethod.Invoke(target, [newValue]);
        }

    }
}

class Demo
{
    /* 
    属性变量在编译后的代码是：
    [CompilerGenerated]
    private int <PropValue>k__BackingField;
    public int PropValue
    {
        [CompilerGenerated]
        get
        {
            return <PropValue>k__BackingField;
        }
        [CompilerGenerated]
        set
        {
            <PropValue>k__BackingField = value;
        }
    }
    因此当我们使用 new Demo().PropValue 来访问属性值时本质上是调用了
    自动编译生成的这个 <PropValue>k__BackingField 字段的 getter 方法
    */
    public int PropValue { get; set; }
    // 字段
    public int FieldValue;
}