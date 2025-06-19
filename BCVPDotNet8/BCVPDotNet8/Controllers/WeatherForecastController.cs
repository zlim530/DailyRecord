using AutoMapper;
using BCVPDotNet8.Model;
using BCVPDotNet8.Service;
using BCVPDotNet8.Service.Base;
using Microsoft.AspNetCore.Mvc;

namespace BCVPDotNet8.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBaseService<User, UserVo> _userService;
        // 属性注入必须使用 public 修饰属性
        public IBaseService<User, UserVo> _baseUserService { get; set; }

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
                                        IBaseService<User, UserVo> userService
                                        )
        {
            _logger = logger;
            _userService = userService;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet(Name = "GetUser")]
        public async Task<List<UserVo>> GetUser()
        {
            var userService = new UserService();
            var userList = await userService.Query();
            return userList;
        }

        [HttpGet(Name = "GetBaseUser")]
        public async Task<List<UserVo>> GetBaseUser()
        {
            //var baseService = new BaseService<User, UserVo>(_mapper);
            //return await baseService.Query();
            
            //var userList = await _userService.Query();    // 字段-构造函数依赖注入
            var userList = await _baseUserService.Query();// 属性依赖注入
            return userList;
        }
    }
}
