using AutoMapper;
using BCVPDotNet8.Repository;
using SqlSugar;
using System.Linq.Expressions;

namespace BCVPDotNet8.Service.Base
{
    public class BaseService<TEntity, TVo> : IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        private readonly IMapper _mapper;
        private readonly IBaseRepository<TEntity> _baseRepository;
        public ISqlSugarClient Db => _baseRepository.DB;

        public BaseService(IMapper mapper, IBaseRepository<TEntity> baseRepository)
        { 
            _mapper = mapper;
            _baseRepository = baseRepository;
        }

        public async Task<long> Add(TEntity entity)
        {
            return await _baseRepository.Add(entity);
        }

        public async Task<List<TVo>> Query(Expression<Func<TEntity, bool>>? whereExpression = null)
        {
            var entities = await _baseRepository.Query(whereExpression);
            Console.WriteLine($"In BaseService: _baseRepository 实例 HashCode : {_baseRepository.GetHashCode().ToString()}");
            var llout = _mapper.Map<List<TVo>>(entities);
            return llout;
        }


        public async Task<List<TEntity>> QuerySplit(Expression<Func<TEntity, bool>> whereExpression, string orderByFields = null)
        { 
            return await _baseRepository.QuerySplit(whereExpression, orderByFields);
        }

        public async Task<List<long>> AddSplit(TEntity entity)
        {
            return await _baseRepository.AddSplit(entity);
        }

    }
}
