using BCVPDotNet8.Common;
using BCVPDotNet8.Repository.UnitOfWorks;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    /// <summary>
    /// 事务拦截器BlogTranAOP 继承IInterceptor接口
    /// </summary>
    public class TranAOP : IInterceptor
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWorkManage _unitOfWorkManage;
        
        public TranAOP(IUnitOfWorkManage unitOfWorkManage,
                    ILogger<TranAOP> logger            
            )
        {
            _logger = logger;
            _unitOfWorkManage = unitOfWorkManage;
        }

        /// <summary>
        /// 实例化IInterceptor唯一方法 
        /// </summary>
        /// <param name="invocation">包含被拦截方法的信息</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            // 对当前方法的特性验证
            // 如果需要验证
            if (method.GetCustomAttribute<UserTranAttribute>(true) is { } uta)
            {
                try
                {
                    // 动态代理实现事务
                    Before(method, uta.Propagation);

                    invocation.Proceed();

                    // 异步获取异常，先执行
                    if (IaAsyncMethod(invocation.Method))
                    {
                        var result = invocation.ReturnValue;
                        if (result is Task)
                        {
                            Task.WaitAll(result as Task);
                        }
                    }

                    //当所有方法执行完成后，执行After。
                    After(method);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    // 如果异常就直接回滚。
                    AfterException(method);
                    throw;
                }
            }
            else
            {
                invocation.Proceed();// 直接执行被拦截方法
            }
        
        }

        private void AfterException(MethodInfo method)
        {
            _unitOfWorkManage.RollbackTran(method);
        }

        private void After(MethodInfo method)
        {
            _unitOfWorkManage.CommitTran(method);
        }

        private static bool IaAsyncMethod(MethodInfo method)
        {
            return
                 method.ReturnType == typeof(Task) ||
                 method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)
             ;
        }

        private void Before(MethodInfo method, Propagation propagation)
        {
            switch (propagation)
            {
                case Propagation.Required:
                    if (_unitOfWorkManage.TranCount <= 0)
                    {
                        _logger.LogDebug($"Begin Transaction");
                        Console.WriteLine($"Begin Transaction");
                        _unitOfWorkManage.BeginTran(method);
                    }
                    break;
                case Propagation.Mandatory:
                    if (_unitOfWorkManage.TranCount <= 0)
                    {
                        throw new Exception("事务传播机制为:[Mandatory],当前不存在事务");
                    }
                    break;
                case Propagation.Nested:
                    _logger.LogDebug($"Begin Transaction");
                    Console.WriteLine($"Begin Transaction");
                    _unitOfWorkManage.BeginTran(method);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(propagation), propagation, null);
            }
        }


        /// <summary>
        /// 获取变量的默认值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
