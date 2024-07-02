using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO.BaseDTO
{
    public abstract class TableBaseDTO
    {
        /// <summary>
        /// 資料表名稱
        /// </summary>
        public abstract string Table { get; }
    }

    /// <summary>
    /// 資料表Key
    /// </summary>
    public class PrimaryKeyAttribute : Attribute { }

    /// <summary>
    /// 資料表欄位
    /// </summary>
    public class TableColumnAttribute : Attribute { }

    /// <summary>
    /// 需要特殊處理的資料類型
    /// </summary>
    public class ColTypeAttribute : Attribute
    {
        private TableColTypeS _colType;
        public ColTypeAttribute(TableColTypeS colType)
        {
            _colType = colType;
        }

        public TableColTypeS ColType { get { return _colType; } }
    }

    //需要特殊處理的格式
    public enum TableColTypeS
    {
        None,
        Date,
        DateTime
    }
}
