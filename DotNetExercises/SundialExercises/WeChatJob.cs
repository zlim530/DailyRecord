using Sundial;

namespace SundialExercises
{
    public class WeChatJob : IJob
    {
        private readonly ILogger<WeChatJob> _logger;

        public WeChatJob(ILogger<WeChatJob> logger)
        {
            _logger = logger;
        }

        public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
        {
            _logger.LogInformation("WeChat job is executing at: {time}", DateTimeOffset.Now);
            // Simulate some work with a delay
            return Task.CompletedTask;
        }
    }
}
