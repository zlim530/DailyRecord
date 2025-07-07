using SqlSugar;

namespace BCVPDotNet8.Model
{
    [Tenant("log")]
    [SplitTable(SplitType.Month)]// 按月自动分表：自带分表支持如下：按年、季、月、周、日
    /*
    按年自动分表格式如下：
    SplitTestTable_20220101
    按月自动分表格式如下，假设现在是5月份：
    SplitTestTable_20220501
    按日自动分表格式如下，假设现在是5月11号：
    SplitTestTable_20220511
    */
    [SugarTable($@"{nameof(AuditSqlLog)}_{{year}}{{month}}{{day}}")]
    //[SugarTable("AuditSqlLog_20231201", "Sql审计日志")]//('数据库表名'，'数据库表备注') 指定映射的数据库表名
    public class AuditSqlLog : BaseLog
    {
    }
}
