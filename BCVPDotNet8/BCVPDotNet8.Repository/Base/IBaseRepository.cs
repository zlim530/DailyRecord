using SqlSugar;

namespace BCVPDotNet8.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class, new()
    {
        ISqlSugarClient DB { get; }
        Task<long> Add(TEntity entity);
        Task<List<TEntity>> Query();
    }
}
