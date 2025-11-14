namespace WhyWeNeedInterface.Services
{
    public class SystemTimeProvider : ITimeProvider
    {
        public int GetHour()
        {
            return DateTime.Now.Hour;
        }
    }
}
