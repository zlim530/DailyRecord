using BCVPDotNet8.Model;
using BCVPDotNet8.Repository;

namespace BCVPDotNet8.Service
{
    public class UserService : IUserService
    {
        public async Task<List<UserVo>> Query()
        { 
            var userRepository = new UserRepository();
            var users = await userRepository.Query();
            return users.Select(u => new UserVo() { UserName = u.Name}).ToList();
        }
    }
}
