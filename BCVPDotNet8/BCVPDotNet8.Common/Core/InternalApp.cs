using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BCVPDotNet8.Common.Core
{
    /// <summary>
    /// 只用于内部初始化使用:获取IServiceCollection、IServiceProvider、IConfiguration等几个重要的内部对象
    /// </summary>
    public static class InternalApp
    {
        internal static IServiceCollection InternalServices;
        /// <summary>
        /// 根服务
        /// </summary>
        internal static IServiceProvider RootServices;
        /// <summary>
        /// 获取Web主机环境
        /// </summary>
        internal static IWebHostEnvironment WebHostEnvironment;
        /// <summary>
        /// 获取泛型主机环境
        /// </summary>
        internal static IHostEnvironment HostEnvironment;
        /// <summary>
        /// 配置对象
        /// </summary>
        internal static IConfiguration Configuration;

        public static void ConfigureApplication(this WebApplicationBuilder web)
        {
            HostEnvironment = web.Environment;
            WebHostEnvironment = web.Environment;
            InternalServices = web.Services;
        }

        public static void ConfigureApplication(this IConfiguration configuration)
        { 
            Configuration = configuration;
        }

        public static void ConfigureApplication(this IHost app)
        { 
            RootServices = app.Services;
        }

    }
}
