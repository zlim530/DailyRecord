using Autofac;
using Autofac.Extras.DynamicProxy;
using BCVPDotNet8.Repository;
using BCVPDotNet8.Repository.UnitOfWorks;
using BCVPDotNet8.Service.Base;
using System.Reflection;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    public class AutofacModuleRegister : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var basePath = AppContext.BaseDirectory;

            var servicesDllFile = Path.Combine(basePath, "BCVPDotNet8.Service.dll");
            var repositoryDllFile = Path.Combine(basePath, "BCVPDotNet8.Repository.dll");

            var apoTypes = new List<Type>() { typeof(ServiceAOP), typeof(TranAOP)};
            builder.RegisterType<ServiceAOP>();
            builder.RegisterType<TranAOP>();

            // 泛型类需要单独注册
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();
            builder.RegisterGeneric(typeof(BaseService<,>)).As(typeof(IBaseService<,>))
                .InstancePerDependency()
                .EnableInterfaceInterceptors()
                .InterceptedBy(apoTypes.ToArray())
                ;

            // 获取 Service.dll 程序集服务并注册
            var assemblysServices = Assembly.LoadFrom(servicesDllFile);
            builder.RegisterAssemblyTypes(assemblysServices)
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired()
                .EnableInterfaceInterceptors() // 只在服务层（Service）注入日志。
                .InterceptedBy(apoTypes.ToArray())
                ;

            // 获取 Repositorty.dll 程序集服务并注册
            var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
            builder.RegisterAssemblyTypes(assemblysRepository)
                .AsImplementedInterfaces()
                .InstancePerDependency()
                .PropertiesAutowired();

            // 事务AOP瞬态注册服务
            builder.RegisterType<UnitOfWorkManage>().As<IUnitOfWorkManage>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope()
                .PropertiesAutowired()
                ;

            // ✅ 注册构建完成后的调试输出（推荐）
            //builder.RegisterBuildCallback(container =>
            //{
            //    Console.WriteLine("\n[Autofac] 打印注册信息（包含 UnitOfWorkManage）：");

            //    foreach (var registration in container.ComponentRegistry.Registrations)
            //    {
            //        var serviceTypes = registration.Services
            //            .Select(s => s.Description)
            //            .ToArray();

            //        var implementationType = registration.Activator.LimitType.FullName;
            //        var hasProxy = implementationType.Contains("Castle.Proxies");

            //        if (serviceTypes.Any(s => s.Contains("IUnitOfWorkManage")) || implementationType.Contains("UnitOfWorkManage"))
            //        {
            //            Console.WriteLine("服务类型: " + string.Join(", ", serviceTypes));
            //            Console.WriteLine("实现类型: " + implementationType);
            //            Console.WriteLine("是否 Castle Proxy: " + hasProxy);
            //            Console.WriteLine(new string('-', 50));
            //        }
            //    }
            //});
        }
    }
}
