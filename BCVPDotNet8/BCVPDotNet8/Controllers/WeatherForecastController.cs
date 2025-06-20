using BCVPDotNet8.Common;
using BCVPDotNet8.Common.Option;
using BCVPDotNet8.Model;
using BCVPDotNet8.Service;
using BCVPDotNet8.Service.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        private readonly IOptions<RedisOptions> _redisOptions;

        // 属性注入必须使用 public 修饰属性
        public IBaseService<User, UserVo> _baseUserService { get; set; }

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
                                        IBaseService<User, UserVo> userService,
                                        IOptions<RedisOptions> redisOptions
                                        )
        {
            _logger = logger;
            _userService = userService;
            _redisOptions = redisOptions;
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

            // 使用这种方法无法避免硬编码
            var redisEnable = AppSettings.app(new string[] { "Redis", "Enable"});
            var redisConnectionString = AppSettings.GetValue("Redis:ConnectionString");
            Console.WriteLine($"Enable:{redisEnable}, ConnectionString:{redisConnectionString}");

            var redisOptions = _redisOptions.Value;
            Console.WriteLine(JsonConvert.SerializeObject(redisOptions));

            return userList;
        }
    }
}
