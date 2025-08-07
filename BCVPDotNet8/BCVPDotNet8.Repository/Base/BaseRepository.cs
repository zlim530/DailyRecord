using BCVPDotNet8.Repository.UnitOfWorks;
using SqlSugar;
using System.Linq.Expressions;
using System.Reflection;

namespace BCVPDotNet8.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        public IUnitOfWorkManage _unitOfWorkManage { get; }
        private readonly SqlSugarScope _dbBase;
        public ISqlSugarClient Db => _db;
        private ISqlSugarClient _db
        {
            get
            { 
                ISqlSugarClient db = _dbBase;

                //修改使用 model备注字段作为切换数据库条件，使用sqlsugar TenantAttribute存放数据库ConnId
                //参考 https://www.donet5.com/Home/Doc?typeId=2246
                var tenantAttr = typeof(TEntity).GetCustomAttribute<TenantAttribute>();
                if (tenantAttr != null)
                {
                    //统一处理 configId 小写
                    db = _dbBase.GetConnectionScope(tenantAttr.configId.ToString().ToLower());
                }

                return db;
            }
        }

        public BaseRepository(IUnitOfWorkManage unitOfWorkManage)
        {
            _unitOfWorkManage = unitOfWorkManage;
            _dbBase = unitOfWorkManage.GetDbClient();
        }

        public ISqlSugarClient DB => _dbBase;


        /// <summary>
        /// 写入实体数据
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<long> Add(TEntity entity)
        {
            var insert = _db.Insertable(entity);
            return await insert.ExecuteReturnSnowflakeIdAsync();
        }

        public async Task<List<TEntity>> Query(Expression<Func<TEntity, bool>> whereExpression = null)
        {
            //await Task.CompletedTask;
            //var data = "[{\"Id\": 530,\"Name\":\"basezlim530\"}]";
            //return JsonConvert.DeserializeObject<List<TEntity>>(data) ?? new List<TEntity>();
            Console.WriteLine($"In BaseRepository: DB.GetHashCode().ToString(): {DB.GetHashCode().ToString()}");
            return await _db.Queryable<TEntity>().WhereIF(whereExpression != null, whereExpression).ToListAsync();
        }


        /// <summary>
        /// 分表查询
        /// </summary>
        /// <param name="whereExpression">条件表达式</param>
        /// <param name="orderByFields">排序字段，如name asc,age desc等</param>
        /// <returns></returns>
        public async Task<List<TEntity>> QuerySplit(Expression<Func<TEntity, bool>> whereExpression, string orderByFields = null)
        {
            return await _db.Queryable<TEntity>()
                            .SplitTable()// 标识启用了分表
                            .OrderByIF(!string.IsNullOrEmpty(orderByFields), orderByFields)
                            .WhereIF(whereExpression != null
                            , whereExpression)
                            .ToListAsync();
        }

        /// <summary>
        /// 写入实体数据
        /// </summary>
        /// <param name="entity">数据实体</param>
        /// <returns></returns>
        public async Task<List<long>> AddSplit(TEntity entity)
        {
            var insert = _db.Insertable(entity).SplitTable();
            // 插入并返回雪花ID并且自动赋值ID　
            return await insert.ExecuteReturnSnowflakeIdListAsync();
        }

        // 多表联查
        public async Task<List<TResult>> QueryMuch<T, T2, T3, TResult>(
          Expression<Func<T, T2, T3, object[]>> joinExpression,
          Expression<Func<T, T2, T3, TResult>> selectExpression,
          Expression<Func<T, T2, T3, bool>> whereLambda = null) where T : class, new()
        {
            if (whereLambda == null)
            {
                return await _db.Queryable(joinExpression).Select(selectExpression).ToListAsync();
            }

            return await _db.Queryable(joinExpression).Where(whereLambda).Select(selectExpression).ToListAsync();
        }
    }
}
