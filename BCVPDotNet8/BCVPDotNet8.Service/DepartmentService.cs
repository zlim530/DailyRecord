using AutoMapper;
using BCVPDotNet8.Common;
using BCVPDotNet8.Model;
using BCVPDotNet8.Repository;
using BCVPDotNet8.Service.Base;

namespace BCVPDotNet8.Service
{
    public class DepartmentService : BaseService<Department, DepartmentVo>, IDepartmentService
    {
        private readonly IBaseRepository<Department> _dal;

        public DepartmentService(IMapper mapper, IBaseRepository<Department> baseRepository) : base(mapper, baseRepository)
        {
            _dal = baseRepository;
        }

        /// <summary>
        /// 测试使用同事务
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        [UserTran(Propagation = Propagation.Required)]
        public async Task<bool> TestTranPropagation2()
        {
            TimeSpan timeSpan = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1,0,0 ,0, DateTimeKind.Utc);
            var id = timeSpan.TotalSeconds.ObjToLong();
            var insertDepartment = await _dal.Add(new Department()
            { 
                Id = id,
                Name = $"department name {id}",
                CodeRelationship = "",
                OrderSort = 0,
                Status = true,
                IsDeleted = false,
                Pid = 0
            });

            await Console.Out.WriteLineAsync($"db context id : {base.Db.ContextID}");

            return true;
        }
    }
}
