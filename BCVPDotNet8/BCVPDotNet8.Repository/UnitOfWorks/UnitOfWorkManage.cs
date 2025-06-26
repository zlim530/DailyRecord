using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Collections.Concurrent;

namespace BCVPDotNet8.Repository.UnitOfWorks
{
    public class UnitOfWorkManage : IUnitOfWorkManage
    {
        private readonly ILogger<UnitOfWorkManage> _logger;
        private readonly ISqlSugarClient _sqlSugarClient;
        public readonly ConcurrentStack<string> TranStack = new();

        public UnitOfWorkManage(ISqlSugarClient sqlSugarClient,
                                ILogger<UnitOfWorkManage> logger)
        {
            _logger = logger;
            _sqlSugarClient = sqlSugarClient;
        }

        public void BeginTran()
        {
            lock (this) 
            {
                GetDbClient().BeginTran();
            }
        }

        public void CommitTran()
        {
            lock (this) 
            { 
                GetDbClient().CommitTran();
            }
        }

        public UnitOfWork CreateUnitOfWork()
        {
            UnitOfWork uow = new()
            { 
                Logger = _logger,
                Db = _sqlSugarClient,
                Tenant = (ITenant)_sqlSugarClient,
                IsTran = true
            };

            uow.Db.Open();
            uow.Tenant.BeginTran();
            _logger.LogDebug("UnitOfWork Begin");
            return uow;
        }

        /// <summary>
        /// 获取DB，保证唯一性
        /// </summary>
        /// <returns></returns>
        public SqlSugarScope GetDbClient()
        {
            // 必须要as，后边会用到切换数据库操作
            return _sqlSugarClient as SqlSugarScope;
        }

        public void RollbackTran()
        {
            lock (this)
            {
                GetDbClient().RollbackTran();
            }
        }
    }
}
