using BCVPDotNet8.Common;
using BCVPDotNet8.Model;
using BCVPDotNet8.Repository.UnitOfWorks;
using BCVPDotNet8.Service;
using BCVPDotNet8.Service.Base;
using Microsoft.AspNetCore.Mvc;

namespace BCVPDotNet8.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly IBaseService<Role, RoleVo> _roleService;
        private readonly IUserService _userService;
        private readonly IBaseService<AuditSqlLog, AuditSqlLogVo> _auditSqlLogService;
        private readonly IUnitOfWorkManage _unitOfWorkManage;

        public TransactionController(IBaseService<Role, RoleVo> roleService,
                                    IUserService userService,
                                    IBaseService<AuditSqlLog, AuditSqlLogVo> auditSqlLogService,
                                    IUnitOfWorkManage unitOfWorkManage)
        {
            _roleService = roleService;
            _userService = userService;
            _auditSqlLogService = auditSqlLogService;
            _unitOfWorkManage = unitOfWorkManage;
        }

        [HttpGet]
        public async Task<string> GetRoleTran()
        {
            try
            {
                Console.WriteLine($"Begin Transaction");

                //_unitOfWorkManage.BeginTran();
                using var uow = _unitOfWorkManage.CreateUnitOfWork();
                var roles = await _roleService.Query();
                Console.WriteLine($"1 first time: the count of role is: {roles.Count}");

                Console.WriteLine($"insert a data into the table role now.");
                TimeSpan timeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var insertPassword = await _roleService.Add(new Role()
                {
                    Id = timeSpan.TotalSeconds.ObjToLong(),
                    IsDeleted = false,
                    Name = "role name",
                });

                var roles2 = await _roleService.Query();
                Console.WriteLine($"2 second time: the count of role is: {roles2.Count}");

                int ex = 0;
                Console.WriteLine($"There's an exception!");
                int throwEx = 1 / ex;

                uow.Commit();
                //_unitOfWorkManage.CommitTran();
            }
            catch (Exception)
            {
                var roles3 = await _roleService.Query();
                Console.WriteLine($"3 second time: the count of role is: {roles3.Count}");
            }

            return await Task.FromResult("OK");
        }


        [HttpGet]
        public async Task<bool> TestTranPropagation()
        {
            return await _userService.TestTranPropagation();
        }


        //[HttpGet]
        //public async Task<List<AuditSqlLogVo>> GetAuditSqlLogList()
        //{
        //    return await _auditSqlLogService.Query();
        //}


        [HttpGet]
        public async Task<List<AuditSqlLog>> GetAuditSqlLogListByDateTime()
        {
            //TimeSpan timeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            //var id = timeSpan.TotalSeconds.ObjToLong();
            //await _auditSqlLogService.AddSplit(new AuditSqlLog()
            //{
            //    Id = id,
            //    //DateTime = DateTime.Now// 如果当前数据库中不存在当前月份的表则会新建一张表
            //    DateTime = Convert.ToDateTime("2023-12-23")
            //});
            return await _auditSqlLogService.QuerySplit(d => /*true*/d.DateTime <= Convert.ToDateTime("2023-12-24"));// 查询时不同表的数据都可以查到
        }
    }
}
