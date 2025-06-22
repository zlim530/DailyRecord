
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BCVPDotNet8.Common;
using BCVPDotNet8.Common.Core;
using BCVPDotNet8.Extensions;
using BCVPDotNet8.Extensions.ServiceExtensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BCVPDotNet8
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>(builder => {
                    builder.RegisterModule<AutofacModuleRegister>();
                    builder.RegisterModule<AutofacPropertityModuleReg>();
                })
                .ConfigureAppConfiguration((hostingContext, config) => 
                {
                    hostingContext.Configuration.ConfigureApplication();
                })
                ;

            // Add services to the container.

            // 属性注入激活控制器
            // ASP.NET Core默认不使用DI获取Controller，是因为DI容器构建完成后就不能变更了，但是Controller是可能有动态加载的需求的。
            // 需要使用IControllerActivator开启Controller的属性注入，默认不开启。
            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(new[] { typeof(AutoMapperConfig).Assembly });
            AutoMapperConfig.RegisterMappings();

            //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            //builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));

            // 配置：二选一即可
            // 配置 AppSettings 类服务注入
            builder.Services.AddSingleton(new AppSettings(builder.Configuration));
            // 配置 Option 类
            builder.Services.AddAllOptionRegister();
            builder.ConfigureApplication();

            var app = builder.Build();

            app.ConfigureApplication();
            app.UseApplicationSetup();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
