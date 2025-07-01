using SqlSugar;

namespace BCVPDotNet8.Service.Base
{
    public interface IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        ISqlSugarClient Db { get; }

        Task<long> Add(TEntity entity);
        Task<List<TVo>> Query();
    }
}
