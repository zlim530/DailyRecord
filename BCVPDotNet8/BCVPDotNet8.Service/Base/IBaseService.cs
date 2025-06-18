namespace BCVPDotNet8.Service.Base
{
    internal interface IBaseService<TEntity, TVo> where TEntity : class, new()
    {
        Task<List<TVo>> Query();
    }
}
