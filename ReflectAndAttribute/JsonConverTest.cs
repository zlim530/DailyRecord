using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReflectAndAttribute;

class JsonConverTest
{
    static void Main001(string[] args)
    {

        Employee employee = new Employee()
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateTime(1990, 1, 1),
            Address = "123 Main St",
            PhoneNumber = "555-1234",
            Email = "john.doe@example.com",
            Salary = 50000.00m,
            IsManager = true,
            Skills = new List<string> { "C#", "SQL", "JavaScript" }
        };

        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
        };

        string json = JsonConvert.SerializeObject(employee, settings);
        Console.WriteLine($"{json}");
        
    }
}


class Employee
{
    [JsonIgnore]
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age => DateTime.Today.Year - DateOfBirth.Year;
    [JsonProperty("Birthday")]
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public decimal Salary { get; set; }
    public bool IsManager { get; set; }
    public List<string> Skills { get; set; }
}
