using SqlSugar;
using System.Reflection;

namespace BCVPDotNet8.Repository.UnitOfWorks
{
    public interface IUnitOfWorkManage
    {
        SqlSugarScope GetDbClient();
        int TranCount { get; }
        void BeginTran();
        void BeginTran(MethodInfo method);
        void CommitTran();
        void CommitTran(MethodInfo method);
        void RollbackTran();
        void RollbackTran(MethodInfo method);
        UnitOfWork CreateUnitOfWork();
    }
}
