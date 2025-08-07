using SqlSugar;
using System.Linq.Expressions;

namespace BCVPDotNet8.Service.Base
{
    public interface IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        ISqlSugarClient Db { get; }

        Task<long> Add(TEntity entity);
        Task<List<long>> AddSplit(TEntity entity);
        Task<List<TVo>> Query(Expression<Func<TEntity, bool>>? whereExpression = null);
        Task<List<TEntity>> QuerySplit(Expression<Func<TEntity, bool>> whereExpression, string orderByFields = null);
    }
}
