using BCVPDotNet8.Model;
using Newtonsoft.Json;

namespace BCVPDotNet8.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        public async Task<List<TEntity>> Query()
        {
            await Task.CompletedTask;
            var data = "[{\"Id\": 530,\"Name\":\"basezlim530\"}]";
            return JsonConvert.DeserializeObject<List<TEntity>>(data) ?? new List<TEntity>();
        }
    }
}
