using BCVPDotNet8.Model;
using BCVPDotNet8.Service.Base;

namespace BCVPDotNet8.Service
{
    public interface IUserService : IBaseService<SysUserInfo, UserVo>
    {
        Task<bool> TestTranPropagation();
        Task<string> GetUserRoleNameStr(string loginName, string loginPwd);
        Task<List<RoleModulePermission>> RoleModuleMaps();
    }
}
