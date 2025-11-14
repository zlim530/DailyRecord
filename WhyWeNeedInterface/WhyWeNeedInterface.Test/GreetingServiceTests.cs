using Moq;
using WhyWeNeedInterface.Services;

namespace WhyWeNeedInterface.Test
{
    public class GreetingServiceTests
    {
        [Fact]
        public void Greeting_Includes_Name_And_Correct_Period()
        {
            var provider = new SystemTimeProvider();
            var service = new GreetingService(provider);
            var name = "Lim";
            var message = service.GetGreetingMessage(name);

            Assert.Contains(name, message);

            var hour = DateTime.Now.Hour;
            if (hour < 12)
            {
                Assert.StartsWith("Good Morning", message);
            }
            else if (hour < 18)
            {
                Assert.StartsWith("Good Afternoon", message);
            }
            else
            {
                Assert.StartsWith("Good Evening", message);
            }
        }

        [Fact]
        public void GreetingService_Uses_TimeProvider()
        {
            var mockProvider = new Mock<ITimeProvider>();
            mockProvider.Setup(p => p.GetHour()).Returns(10);
            var service = new GreetingService(mockProvider.Object);
            var message = service.GetGreetingMessage("Lim");
            Assert.StartsWith("Good Morning", message);
        }

        //class FakeTimeProvider : ITimeProvider
        //{
        //    public int GetHour() => 10;
        //}

    }
}