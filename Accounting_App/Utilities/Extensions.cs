using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.Utilities
{
    public static class Extensions
    {
        public static string GetDescriptionText<Enum>(this Enum source)
        {
            FieldInfo fi = source.GetType().GetField(source.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;
            else
                return source.ToString();
        }

        public static string NullToString(this object Value)
        {
            return Value == null ? "" : Value.ToString();
        }

        #region DateTime

        /// <summary>取得yyyy-MM-dd HH:mm:ss格式字串</summary>
        public static string GetFullDateTime(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /// <summary>取得yyyy-MM-dd格式字串</summary>
        public static string GetFullDate(this DateTime datetime)
        {
            return datetime.ToString("yyyy-MM-dd");
        }
        /// <summary>取得yyyy-MM-dd格式字串</summary>
        public static string GetFullDate(this DateTime? datetime)
        {
            DateTime dt = datetime ?? DateTime.MinValue;
            if (dt != DateTime.MinValue)
                return dt.ToString("yyyy-MM-dd");
            else
                return string.Empty;
        }

        #endregion
    }
}
