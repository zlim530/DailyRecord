using BCVPDotNet8.Model;
using Newtonsoft.Json;

namespace BCVPDotNet8.Repository
{
    public class UserRepository : IUserRepository
    {
        public async Task<List<User>> Query()
        {
            await Task.CompletedTask;
            var data = "[{\"Id\": 530,\"Name\":\"zlim530\"}]";
            return JsonConvert.DeserializeObject<List<User>>(data) ?? new List<User>();
        }
    }
}
