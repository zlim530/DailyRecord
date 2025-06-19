namespace BCVPDotNet8.Service.Base
{
    public interface IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        Task<List<TVo>> Query();
    }
}
