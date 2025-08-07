using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCVPDotNet8.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize("Permission")]
    public class UserController: ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        // GET: api/User
        [HttpGet]
        public string Get(int page = 1, string key = "")
        {
            _logger.LogInformation(key, page);
            return "OK!";
        }

        // GET: api/User/5
        [HttpGet]
        public string GetById(string id)
        {
            _logger.LogInformation("test wrong");
            return "value";
        }
    }
}
