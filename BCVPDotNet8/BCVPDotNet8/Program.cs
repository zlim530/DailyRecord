
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BCVPDotNet8.Common;
using BCVPDotNet8.Common.Core;
using BCVPDotNet8.Extensions;
using BCVPDotNet8.Extensions.ServiceExtensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            #region IOC

            // 属性注入激活控制器
            // ASP.NET Core默认不使用DI获取Controller，是因为DI容器构建完成后就不能变更了，但是Controller是可能有动态加载的需求的。
            // 需要使用IControllerActivator开启Controller的属性注入，默认不开启。
            builder.Services.AddControllers()
                    .AddControllersAsServices() // 也即 controller 的创建方式从容器中创建，而控制器中的构造函数依赖的服务或者属性也是从控制反转的容器中创建的
                    ;
            // 核心原理其实就是把控制器 controller 当做一个服务
            // IControllerActivator的默认实现不是ServiceBasedControllerActivator，而是DefaultControllerActivator。
            // 控制器本身不是由依赖注入容器生成的，只不过是构造函数里的依赖是从容器里拿出来的，控制器不是容器生成的，所以他的属性也不是容器生成的。
            // 为了改变默认实现DefaultControllerActivator，所以使用ServiceBasedControllerActivator。
            // IControllerActivator源码地址：https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Controllers/IControllerActivator.cs
            // 查看DefaultControllerActivator 和ServiceBasedControllerActivator源码发现：
            // DefaultControllerActivator是由ITypeActivatorCache.CreateInstance创建对象。
            // ServiceBasedControllerActivator是由actionContext.HttpContext.RequestServices创建对象。
            // 通过改变Controllers的创建方式来实现属性注入，将Controller的创建都由容器容器创建。
            // Controller由容器创建完成，所以他的属性也是容器创建的，从而实现属性依赖注入：属性修饰词必须是public

            //builder.Services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(new[] { typeof(AutoMapperConfig).Assembly });
            AutoMapperConfig.RegisterMappings();

            //builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            //builder.Services.AddScoped(typeof(IBaseService<,>), typeof(BaseService<,>));
            #endregion

            #region 配置

            // 配置：二选一即可
            // 配置 AppSettings 类服务注入
            builder.Services.AddSingleton(new AppSettings(builder.Configuration));
            // 配置 Option 类
            builder.Services.AddAllOptionRegister();
            builder.ConfigureApplication();
            #endregion

            // ORM(Object-Relational Mapping)：对象关系映射：它把 数据库中的表 映射为 编程语言中的类，把表中的记录映射为类的对象，让你可以用面向对象的方式操作数据库
            builder.Services.AddSqlsugarSetup();

            // 缓存
            builder.Services.AddCacheSetup();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = "Zlim.Core",
                        ValidAudience = "zlim",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sdfsdfsrty45634kkhllghtdgdfss345t678fs"))
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("Claim", policy => policy.RequireClaim("iss","Zlim.Core").Build());
                options.AddPolicy("User", policy => policy.RequireRole("User").Build());
                options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin").Build());
                options.AddPolicy("SystemOrAdmin", policy => policy.RequireRole("SuperAdmin", "System"));
            });
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
