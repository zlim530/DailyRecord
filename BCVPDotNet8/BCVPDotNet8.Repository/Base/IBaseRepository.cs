using SqlSugar;

namespace BCVPDotNet8.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class, new()
    {
        ISqlSugarClient DB { get; }
        Task<List<TEntity>> Query();
    }
}
