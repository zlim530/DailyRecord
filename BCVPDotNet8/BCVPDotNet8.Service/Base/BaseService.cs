using AutoMapper;
using BCVPDotNet8.Repository;

namespace BCVPDotNet8.Service.Base
{
    public class BaseService<TEntity, TVo> : IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        private readonly IMapper _mapper;

        public BaseService(IMapper mapper)
        { 
            _mapper = mapper;
        }

        public async Task<List<TVo>> Query()
        {
            var baseRepo = new BaseRepository<TEntity>();
            var entities = await baseRepo.Query();
            var llout = _mapper.Map<List<TVo>>(entities);
            return llout;
        }
    }
}
