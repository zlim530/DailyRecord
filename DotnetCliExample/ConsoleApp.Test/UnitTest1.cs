using ConsoleApp;

namespace ConsoleApp.Test;

public class UnitTest1
{
    [Fact]
    public void Test_StringUtils_Reversed()
    {
        var testString = "Hello World!";
        var result = StringUtils.Reversed(testString);
        Assert.Equal("!dlroW olleH", result);
    }
}