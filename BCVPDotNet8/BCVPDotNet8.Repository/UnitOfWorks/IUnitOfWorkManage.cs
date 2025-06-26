﻿using SqlSugar;

namespace BCVPDotNet8.Repository.UnitOfWorks
{
    public interface IUnitOfWorkManage
    {
        SqlSugarScope GetDbClient();
        void BeginTran();
        void CommitTran();
        void RollbackTran();
        UnitOfWork CreateUnitOfWork();
    }
}
