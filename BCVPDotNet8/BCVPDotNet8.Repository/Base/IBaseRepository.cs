using BCVPDotNet8.Model;

namespace BCVPDotNet8.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class, new()
    {
        Task<List<TEntity>> Query();
    }
}
