using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
#if DEBUG
Console.WriteLine("DEBUG is defined");
#else
Console.WriteLine("DEBUG is not defined");
#endif

var student = new Student(1, "John", "A", 80);
var json = JsonConvert.SerializeObject(student);
Console.WriteLine(json);

public record Student(int Id, string Name, string Class, int Score);