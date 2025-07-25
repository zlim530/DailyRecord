﻿using SqlSugar;

namespace BCVPDotNet8.Model
{
    public abstract class BaseLog : RootEntityTkey<long>
    {
        [SplitField]// SplitField，设置分表字段，根据创建时间来分表。
        public DateTime? DateTime { get; set; }

        [SugarColumn(IsNullable = true)]
        public string Level { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "longtext,text,clob")]
        public string Message { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "longtext,text,clob")]
        public string MessageTemplate { get; set; }

        [SugarColumn(IsNullable = true, ColumnDataType = "longtext,text,clob")]
        public string Properties { get; set; }
    }
}
