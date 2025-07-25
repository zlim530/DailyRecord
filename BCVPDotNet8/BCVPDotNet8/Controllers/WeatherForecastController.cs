﻿using BCVPDotNet8.Common;
using BCVPDotNet8.Common.Caches;
using BCVPDotNet8.Common.Core;
using BCVPDotNet8.Common.Option;
using BCVPDotNet8.Model;
using BCVPDotNet8.Service;
using BCVPDotNet8.Service.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BCVPDotNet8.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    //[Authorize]// 授权认证[Authorize]入门
    // 多个 Authorize 同时开启标识需要同时满足才可以访问接口；如果想实现 SuperAdmin or Claim 的效果可以在 Program 中的 AddPolicy("SystemOrAdmin") 设置
    //[Authorize(Roles = "SuperAdmin")]// 基于角色的授权认证，token 中的角色信息必须是 SuperAdmin 才可以访问接口
    //[Authorize(Policy = "Claim")]// 基于政策"Claim"授权，具体要求见 Program 中的 AddPolicy
    [Authorize("Permission")]// 可以省略 Policy = 因为 Authorize 特性类中有 public AuthorizeAttribute(string policy); 构造函数
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IBaseService<SysUserInfo, UserVo> _userService;
        private readonly IBaseService<Role, RoleVo> _roleService;
        private readonly IOptions<RedisOptions> _redisOptions;
        private readonly ICaching _caching;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // 属性注入必须使用 public 修饰属性
        public IBaseService<SysUserInfo, UserVo> _baseUserService { get; set; }

        public WeatherForecastController(ILogger<WeatherForecastController> logger,
                                        IBaseService<SysUserInfo, UserVo> userService,
                                        IBaseService<Role, RoleVo> roleService,
                                        IOptions<RedisOptions> redisOptions,
                                        ICaching caching,
                                        IHttpContextAccessor httpContextAccessor
                                        )
        {
            _logger = logger;
            _userService = userService;
            _roleService = roleService;
            _redisOptions = redisOptions;
            _caching = caching;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            // 获取 token 中的 Claims
            var httpContext = _httpContextAccessor.HttpContext?.User.Claims.ToList();
            foreach (var item in httpContext)
            {
                //Console.WriteLine($"{item.Type} : {item.Value}");
                /*
                http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier : user123
                jti : 4de1b201-5903-49b0-a92a-2231e229709a
                http://schemas.microsoft.com/ws/2008/06/identity/claims/role : SuperAdmin
                exp : 1752046461
                iss : Zlim.Core
                aud : zlim
                */
            }

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
            //var userService = new UserService();
            var userService = _userService;
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
            var userServiceObjNew = App.GetService<IBaseService<SysUserInfo, UserVo>>(false);
            var userList = await userServiceObjNew.Query();
            var redisOptions = App.GetOptions<RedisOptions>();
            Console.WriteLine(JsonConvert.SerializeObject(redisOptions));

            Console.WriteLine("api request end ... ");
            return userList;
        }


        [HttpGet(Name = "GetBaseRole")]
        public async Task<List<RoleVo>> GetBaseRole()
        {
            Console.WriteLine("Current dir: " + Environment.CurrentDirectory);

            var roleList = await _roleService.Query();
            Console.WriteLine("api request end...");
            return roleList;
        }
    }
}
