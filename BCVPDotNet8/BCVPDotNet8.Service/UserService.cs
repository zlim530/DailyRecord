using AutoMapper;
using BCVPDotNet8.Common;
using BCVPDotNet8.Model;
using BCVPDotNet8.Repository;
using BCVPDotNet8.Service.Base;

namespace BCVPDotNet8.Service
{
    public class UserService : BaseService<SysUserInfo, UserVo>, IUserService
    {
        private readonly IDepartmentService _departmentService;
        public UserService(IMapper mapper, 
                            IBaseRepository<SysUserInfo> baseRepository,
                            IDepartmentService departmentService
                            ) 
        : base(mapper, baseRepository)
        {
            _departmentService = departmentService;
        }

        public async Task<List<UserVo>> Query()
        { 
            var userRepository = new UserRepository();
            var users = await userRepository.Query();
            return users.Select(u => new UserVo() { UserName = u.Name}).ToList();
        }

        /// <summary>
        /// 测试使用同事务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        //如果有一个[UseTran(Propagation = Propagation.Required)] 就开启事务
        [UserTran(Propagation = Propagation.Required)]
        public async Task<bool> TestTranPropagation()
        {
            await Console.Out.WriteLineAsync($"db context id : {base.Db.ContextID}");
            var sysUserInfos = await base.Query();

            TimeSpan timeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var id = timeSpan.TotalSeconds.ObjToLong();
            var insertSysUserInfos = await base.Add(new SysUserInfo() 
            { 
                Id = id,
                Name = $"user name {id}",
                Status = 0,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                CriticalModifyTime = DateTime.Now,
                LastErrorTime = DateTime.Now,
                ErrorCount = 0,
                Enable = true,
                TenantId = 0,
            });

            //执行完一个事务后再执行下一个事务。
            await _departmentService.TestTranPropagation2();

            return true;
        }
    }
}
