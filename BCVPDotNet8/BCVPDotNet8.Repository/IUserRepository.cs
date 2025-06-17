using BCVPDotNet8.Model;

namespace BCVPDotNet8.Repository
{
    internal interface IUserRepository
    {
        Task<List<User>> Query();
    }
}
