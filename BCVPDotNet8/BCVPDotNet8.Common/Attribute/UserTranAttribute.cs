namespace BCVPDotNet8.Common
{
    /// <summary>
    /// 这个Attribute就是使用时候的验证，把它添加到需要执行事务的方法中，即可完成事务的操作
    /// </summary>
    public class UserTranAttribute :  Attribute
    {
        /// <summary>
        /// 事务传播方式
        /// </summary>
        public Propagation Propagation { get; set; } = Propagation.Required;
    }
}
