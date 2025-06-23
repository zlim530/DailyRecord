
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

            // ����ע�뼤�������
            // ASP.NET CoreĬ�ϲ�ʹ��DI��ȡController������ΪDI����������ɺ�Ͳ��ܱ���ˣ�����Controller�ǿ����ж�̬���ص�����ġ�
            // ��Ҫʹ��IControllerActivator����Controller������ע�룬Ĭ�ϲ�������
            builder.Services.AddControllers()
                    .AddControllersAsServices() // Ҳ�� controller �Ĵ�����ʽ�������д��������������еĹ��캯�������ķ����������Ҳ�Ǵӿ��Ʒ�ת�������д�����
                    ;
            // ����ԭ����ʵ���ǰѿ����� controller ����һ������
            // IControllerActivator��Ĭ��ʵ�ֲ���ServiceBasedControllerActivator������DefaultControllerActivator��
            // ������������������ע���������ɵģ�ֻ�����ǹ��캯����������Ǵ��������ó����ģ������������������ɵģ�������������Ҳ�����������ɵġ�
            // Ϊ�˸ı�Ĭ��ʵ��DefaultControllerActivator������ʹ��ServiceBasedControllerActivator��
            // IControllerActivatorԴ���ַ��https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Controllers/IControllerActivator.cs
            // �鿴DefaultControllerActivator ��ServiceBasedControllerActivatorԴ�뷢�֣�
            // DefaultControllerActivator����ITypeActivatorCache.CreateInstance��������
            // ServiceBasedControllerActivator����actionContext.HttpContext.RequestServices��������
            // ͨ���ı�Controllers�Ĵ�����ʽ��ʵ������ע�룬��Controller�Ĵ���������������������
            // Controller������������ɣ�������������Ҳ�����������ģ��Ӷ�ʵ����������ע�룺�������δʱ�����public

            //builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(new[] { typeof(AutoMapperConfig).Assembly });
            AutoMapperConfig.RegisterMappings();

            //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            //builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));

            // ���ã���ѡһ����
            // ���� AppSettings �����ע��
            builder.Services.AddSingleton(new AppSettings(builder.Configuration));
            // ���� Option ��
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
