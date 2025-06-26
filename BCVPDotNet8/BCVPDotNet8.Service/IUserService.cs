using BCVPDotNet8.Model;

namespace BCVPDotNet8.Service
{
    public interface IUserService
    {
        Task<List<UserVo>> Query();
    }
}
