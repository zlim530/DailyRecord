using SqlSugar;
using System.Linq.Expressions;

namespace BCVPDotNet8.Repository
{
    public interface IBaseRepository<TEntity> where TEntity : class, new()
    {
        ISqlSugarClient DB { get; }
        Task<long> Add(TEntity entity);
        Task<List<long>> AddSplit(TEntity entity);
        Task<List<TEntity>> Query();
        Task<List<TEntity>> QuerySplit(Expression<Func<TEntity, bool>> whereExpression, string orderByFields = null);
    }
}
