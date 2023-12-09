namespace ConsoleApp;

public static class StringUtils
{
    public static string Reversed(this string  s)
    {
        var chars = s.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}