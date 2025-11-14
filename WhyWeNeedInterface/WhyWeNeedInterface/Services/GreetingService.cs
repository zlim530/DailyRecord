namespace WhyWeNeedInterface.Services
{
    public class GreetingService
    {
        private readonly ITimeProvider _timeProvider;

        public GreetingService(ITimeProvider timeProvider/*, bool isTrue*/)
        {
            _timeProvider = timeProvider;
        }

        public string GetGreetingMessage(string name)
        {
            var hour = _timeProvider.GetHour();
            return hour switch
            {
                < 12 => $"Good Morning, {name}",
                < 18 => $"Good Afternoon, {name}",
                _ => $"Good Evening, {name}"
            };
        }
    }
}
