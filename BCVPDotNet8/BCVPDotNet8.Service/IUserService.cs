using BCVPDotNet8.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCVPDotNet8.Service
{
    internal interface IUserService
    {
        Task<List<UserVo>> Query();
    }
}
