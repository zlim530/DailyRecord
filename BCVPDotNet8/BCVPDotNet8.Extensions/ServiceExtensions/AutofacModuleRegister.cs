using Autofac;
using Autofac.Extras.DynamicProxy;
using BCVPDotNet8.Repository;
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

            var apoTypes = new List<Type>() { typeof(ServiceAOP)};
            builder.RegisterType<ServiceAOP>();

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
        }
    }
}
