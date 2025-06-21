using BCVPDotNet8.Common.Option;
using Microsoft.Extensions.DependencyInjection;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    public static class AllOptionRegister
    {
        public static void AddAllOptionRegister(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            foreach (var optionType in typeof(ConfigurableOptions).Assembly.GetTypes().Where(s => !s.IsInterface && typeof(IConfigurableOptions).IsAssignableFrom(s)))
            {
                services.AddConfigurableOptions(optionType);
            }

        }
    }
}
