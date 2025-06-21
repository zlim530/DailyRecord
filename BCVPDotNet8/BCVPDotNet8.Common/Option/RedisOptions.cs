namespace BCVPDotNet8.Common.Option
{
    /// <summary>
    /// Redis缓存配置选项类
    /// </summary>
    public class RedisOptions : IConfigurableOptions
    {
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string ConnectionString { get; set; }
        /// <summary>
        /// 键值前缀
        /// </summary>
        public string InstanceName { get; set; }
    }
}
