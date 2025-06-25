using AutoMapper;
using BCVPDotNet8.Model;

namespace BCVPDotNet8.Extensions.ServiceExtensions
{
    public class CustomerProfile : Profile
    {
        /// <summary>
        /// 配置构造函数，用来创建关系映射
        /// </summary>
        public CustomerProfile()
        {
            CreateMap<User, UserVo>()
                .ForMember(a => a.UserName, o => o.MapFrom(d => d.Name));
            CreateMap<UserVo, User>()
                .ForMember(a => a.Name, o => o.MapFrom(d => d.UserName));

            CreateMap<Role, RoleVo>()
                .ForMember(a => a.RoleName, o => o.MapFrom(s => s.Name));
            CreateMap<RoleVo, Role>()
                .ForMember(a => a.Name, o => o.MapFrom(d => d.RoleName));

        }
    }
}
