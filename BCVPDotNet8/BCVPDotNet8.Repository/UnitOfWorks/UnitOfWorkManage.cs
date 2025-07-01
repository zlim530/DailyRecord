using BCVPDotNet8.Common;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Collections.Concurrent;
using System.Reflection;

namespace BCVPDotNet8.Repository.UnitOfWorks
{
    public class UnitOfWorkManage : IUnitOfWorkManage
    {
        private readonly ILogger<UnitOfWorkManage> _logger;
        private readonly ISqlSugarClient _sqlSugarClient;
        //ConcurrentStack是线程安全的后进先出(LIFO：栈) 集合。
        public readonly ConcurrentStack<string> TranStack = new();
        private int _tranCount { get; set; }

        public int TranCount => _tranCount;

        public UnitOfWorkManage(ISqlSugarClient sqlSugarClient,
                                ILogger<UnitOfWorkManage> logger)
        {
            _logger = logger;
            _sqlSugarClient = sqlSugarClient;
            _tranCount = 0;
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


        public void BeginTran()
        {
            lock (this) 
            {
                _tranCount++;
                GetDbClient().BeginTran();
            }
        }
        public void BeginTran(MethodInfo method)
        {
            lock (this)
            {
                //第一个方法进来后开启事务，将方法加入队列。多个方法加入，只会在第一次开启事务。
                GetDbClient().BeginTran();
                TranStack.Push(method.GetFullName());
                _tranCount = TranStack.Count;
            }
        }

        public void CommitTran()
        {
            lock (this) 
            {
                _tranCount--;
                if (_tranCount == 0)
                {
                    try
                    {
                        GetDbClient().CommitTran();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        GetDbClient().RollbackTran();
                    }
                }
            }
        }

        /// <summary>
        /// 获取栈中第一个方法。然后提交事务;
        /// 如果异常就回滚事务。最后再从栈中第一个方法开始全部移除，直到删除完成。
        /// </summary>
        /// <param name="method"></param>
        public void CommitTran(MethodInfo method)
        {
            lock (this)
            {
                string result = "";
                while (!TranStack.IsEmpty && !TranStack.TryPeek(out result))
                {
                    Thread.Sleep(1);
                }

                if (result == method.GetFullName())
                {
                    try
                    {
                        GetDbClient().CommitTran();

                        _logger.LogDebug($"Commit Transaction");
                        Console.WriteLine("Commit Transaction");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        GetDbClient().RollbackTran();
                        _logger.LogDebug($"Commit Error, Rollback Transaction");
                    }
                    finally
                    {
                        while (!TranStack.TryPop(out _))
                        {
                            Thread.Sleep(1);
                        }

                        _tranCount = TranStack.Count;
                    }
                }
            }
        }
        

        public void RollbackTran()
        {
            lock (this)
            {
                _tranCount--;
                GetDbClient().RollbackTran();
            }
        }

        //移除所有方法并回滚。
        public void RollbackTran(MethodInfo method)
        {
            lock (this)
            {
                string result = "";
                while (!TranStack.IsEmpty && !TranStack.TryPeek(out result))
                {
                    Thread.Sleep(1);
                }

                if (result == method.GetFullName())
                {
                    GetDbClient().RollbackTran();
                    _logger.LogDebug($"Rollback Transaction");
                    Console.WriteLine($"Rollback Transaction");
                    while (!TranStack.TryPop(out _))
                    {
                        Thread.Sleep(1);
                    }

                    _tranCount = TranStack.Count;
                }
            }
        }
    }
}
