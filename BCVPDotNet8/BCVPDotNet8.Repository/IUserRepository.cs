using BCVPDotNet8.Model;

namespace BCVPDotNet8.Repository
{
    public interface IUserRepository
    {
        Task<List<SysUserInfo>> Query();
        Task<List<RoleModulePermission>> RoleModuleMaps();
    }
}
