using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnumExample
{
    public class Program
    {
        static void Main(string[] args)
        {

            // Enum 类型在内存中使用整数进行存储
            //int i = (int)IncomeType.Rent;
            //Console.WriteLine(i);
            //IncomeType t1 = (IncomeType)666;
            //Console.WriteLine(t1);
            //Console.WriteLine(t1.ToString());
            //IncomeType tnot = Enum.Parse<IncomeType>("112");
            //Console.WriteLine(Enum.IsDefined(tnot));
            //IncomeType tin = Enum.Parse<IncomeType>("Rent");
            //Console.WriteLine(Enum.IsDefined(tin));


            Income[] incomes = JsonSerializer.Deserialize<Income[]>(File.ReadAllText("Input.json"), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                Converters = {new JsonStringEnumConverter(allowIntegerValues:false)}
            })!;

            foreach (var income in incomes)
            {
                // 检查Type是否为定义的枚举值
                if (!Enum.IsDefined(typeof(IncomeType), income.Type))
                {
                    throw new ArgumentException($"输入的Type值 {income.Type} 不在IncomeType枚举定义中！");
                }
                Console.WriteLine($"{ income.Type}: {income.Value }");
            }

        }
    }

    public class Income
    {
        public IncomeType Type { get; set; }
        public double Value { get; set; }
    }

    public enum IncomeType
    {
        Salary,
        Rent,
        Investment,
        Other
    }
}
