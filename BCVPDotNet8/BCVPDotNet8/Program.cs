
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BCVPDotNet8.Common;
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
                });

            // Add services to the container.

            // ����ע�뼤�������
            // ASP.NET CoreĬ�ϲ�ʹ��DI��ȡController������ΪDI����������ɺ�Ͳ��ܱ���ˣ�����Controller�ǿ����ж�̬���ص�����ġ�
            // ��Ҫʹ��IControllerActivator����Controller������ע�룬Ĭ�ϲ�������
            builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(new[] { typeof(AutoMapperConfig).Assembly });
            AutoMapperConfig.RegisterMappings();

            //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            //builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));

            // ���� AppSettings ��
            builder.Services.AddSingleton(new AppSettings(builder.Configuration));

            var app = builder.Build();

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
