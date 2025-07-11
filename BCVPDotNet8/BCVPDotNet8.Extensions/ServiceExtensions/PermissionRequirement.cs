using Microsoft.AspNetCore.Authorization;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    /// <summary>
    /// 自定义授权策略类：自定策略需要实现 IAuthorizationRequirement 接口与 AuthorizationHandler 抽象类。
    /// </summary>
    public class PermissionRequirement : AuthorizationHandler<PermissionRequirement>, IAuthorizationRequirement
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
