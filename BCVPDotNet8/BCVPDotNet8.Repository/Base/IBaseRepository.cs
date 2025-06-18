using BCVPDotNet8.Model;

namespace BCVPDotNet8.Repository
{
    internal interface IBaseRepository<TEntity> where TEntity : class, new()
    {
        Task<List<TEntity>> Query();
    }
}
