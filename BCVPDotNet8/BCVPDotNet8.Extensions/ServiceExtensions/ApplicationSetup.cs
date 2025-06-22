using BCVPDotNet8.Common.Core;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    /// <summary>
    /// 通过事件获取WebApplication的状态。
    /// </summary>
    public static class ApplicationSetup
    {
        public static void UseApplicationSetup(this WebApplication app)
        {
            // 如果需要在程序启动前执行一些操作可以在这里写
            app.Lifetime.ApplicationStarted.Register(() =>
            { 
                App.IsRun = true;
            });

            app.Lifetime.ApplicationStopped.Register(() =>
            {
                App.IsRun = false;
                // 清除日志
                Log.CloseAndFlush();
            });
        }
    }
}
