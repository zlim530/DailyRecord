using SqlSugar;

namespace BCVPDotNet8.Model
{
    /// <summary>
    /// 角色表
    /// </summary>
    public class Role : RootEntityTkey<long>
    {
        /// <summary>
        /// 获取或设置是否禁用，逻辑上的删除，非物理删除：软删除
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public bool? IsDeleted { get; set; }
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderSort { get; set; }
        /// <summary>
        /// 自定义权限的部门ids
        /// </summary>
        public string Dids { get; set; }
        /// <summary>
        /// 权限范围
        /// -1 无任何权限；1 自定义权限；2 本部门；3 本部门及以下；4 仅自己；9 全部；
        /// </summary>
        public int AuthorityScope { get; set; }
        /// <summary>
        /// 是否激活
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// 创建ID
        /// </summary>
        public long? CreateId { get; set; }
        /// <summary>
        ///  创建者
        /// </summary>
        public string CreateBy { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 修改ID
        /// </summary>
        public long? ModifyId { get; set; }
        /// <summary>
        ///  修改者
        /// </summary>
        public string ModifyBy { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyTime { get; set; } = DateTime.Now;
    }
}
