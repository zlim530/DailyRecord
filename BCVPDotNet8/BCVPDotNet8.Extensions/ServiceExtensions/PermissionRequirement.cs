using Microsoft.AspNetCore.Authorization;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public List<PermissionItem> Permissions { get; set; } = new List<PermissionItem>();
    }
}
