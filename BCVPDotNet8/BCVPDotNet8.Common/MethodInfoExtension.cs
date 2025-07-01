using System.Reflection;

namespace BCVPDotNet8.Common
{
    public static class MethodInfoExtension
    {
        public static string GetFullName(this MethodInfo method)
        {
            if (method.DeclaringType == null) return $@"{method.Name}";

            return $"{method.DeclaringType.FullName}.{method.Name}";
        }
    }
}
