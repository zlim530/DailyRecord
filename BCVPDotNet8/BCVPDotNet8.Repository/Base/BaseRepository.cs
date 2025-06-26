using SqlSugar;

namespace BCVPDotNet8.Repository
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class, new()
    {
        private readonly ISqlSugarClient _dbBase;
        public BaseRepository(ISqlSugarClient sqlSugarClient)
        {
            _dbBase = sqlSugarClient;
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
            var insert = _dbBase.Insertable(entity);
            return await insert.ExecuteReturnSnowflakeIdAsync();
        }

        public async Task<List<TEntity>> Query()
        {
            //await Task.CompletedTask;
            //var data = "[{\"Id\": 530,\"Name\":\"basezlim530\"}]";
            //return JsonConvert.DeserializeObject<List<TEntity>>(data) ?? new List<TEntity>();
            Console.WriteLine($"In BaseRepository: DB.GetHashCode().ToString(): {DB.GetHashCode().ToString()}");
            return await _dbBase.Queryable<TEntity>().ToListAsync();
        }
    }
}
