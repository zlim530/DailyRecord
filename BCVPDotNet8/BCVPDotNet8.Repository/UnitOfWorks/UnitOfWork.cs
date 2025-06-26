using Microsoft.Extensions.Logging;
using SqlSugar;

namespace BCVPDotNet8.Repository.UnitOfWorks
{
    public class UnitOfWork : IDisposable
    {

        public ILogger Logger { get; set; }
        public ISqlSugarClient Db { get; internal set; }
        public ITenant Tenant { get; internal set; }

        public bool IsTran { get; set; }

        public bool IsCommit { get; set; }

        public bool IsClose { get; set; }

        /// <summary>
        /// 在释放时自动回滚：
        /// 省去了手动调用 IUnitOfWorkManage.CommitTran() 又在 catch 代码中 IUnitOfWorkManage.RollbackTran()
        /// </summary>
        public void Dispose()
        {
            if (IsTran && !IsCommit)
            {
                Logger.LogDebug("UnitOfWork RollbackTran");
                Tenant.RollbackTran();
            }

            if (Db.Ado.Transaction != null || IsClose)
                return;
            Db.Close();
        }


        public bool Commit()
        {
            if (IsTran && !IsCommit)
            {
                Logger.LogDebug("UnitOfWork CommitTran");
                Tenant.CommitTran();
                IsCommit = true;
            }

            if (Db.Ado.Transaction == null && !IsClose)
            {
                Db.Close();
                IsClose = true;
            }

            return IsCommit;
        }

    }
}