using BCVPDotNet8.Common;
using BCVPDotNet8.Common.Caches;
using BCVPDotNet8.Common.Core;
using BCVPDotNet8.Common.Option;
using BCVPDotNet8.Model;
using BCVPDotNet8.Service;
using BCVPDotNet8.Service.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using BCVPDotNet8.Common.Caches;

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
        private readonly ICaching _caching;

        // 属性注入必须使用 public 修饰属性
        public IBaseService<User, UserVo> _baseUserService { get; set; }

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
                                        IBaseService<User, UserVo> userService,
                                        IOptions<RedisOptions> redisOptions,
                                        ICaching caching
                                        )
        {
            _logger = logger;
            _userService = userService;
            _redisOptions = redisOptions;
            _caching = caching;
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

            var cacheKey = "lim";
            List<string> cacheKeys = await _caching.GetAllCacheKeysAsync();
            await Console.Out.WriteLineAsync("全部keys -->" + JsonConvert.SerializeObject(cacheKeys));
            await Console.Out.WriteLineAsync("添加一个缓存");
            await _caching.SetStringAsync(cacheKey, "lim");
            await Console.Out.WriteLineAsync("全部keys -->" + JsonConvert.SerializeObject(await _caching.GetAllCacheKeysAsync()));
            await Console.Out.WriteLineAsync("当前key内容-->" + JsonConvert.SerializeObject(await _caching.GetStringAsync(cacheKey)));

            await Console.Out.WriteLineAsync("删除key");
            await _caching.RemoveAsync(cacheKey);
            await Console.Out.WriteLineAsync("全部keys -->" + JsonConvert.SerializeObject(await _caching.GetAllCacheKeysAsync()));

            return userList;
        }


        /// <summary>
        /// 通过 App 类获取 UserService：这种方式适用于无法通过构造函数或者属性依赖注入的类，这种方式可以拿到应用程序的所有资源，拿到所有服务
        /// </summary>
        /// <returns></returns>
        [HttpGet(Name = "GetAppUser")]
        public async Task<List<UserVo>> GetAppUser()
        {
            var userServiceObjNew = App.GetService<IBaseService<User, UserVo>>(false);
            var userList = await userServiceObjNew.Query();
            var redisOptions = App.GetOptions<RedisOptions>();
            Console.WriteLine(JsonConvert.SerializeObject(redisOptions));

            Console.WriteLine("api request end ... ");
            return userList;
        }
    }
}
