using Accounting_App.DTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.ValueConverters
{
    public class Vcvt_DateDeault : ValueConverterBase<Vcvt_DateDeault>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //判斷可否轉換
            DateTime r = DateTime.MinValue;
            List<Type> types = new List<Type>() { typeof(DateTime?), typeof(DateTime) };
            if (value != null && types.Contains(value.GetType()))
                r = (DateTime)value;

            //格式：有N代表預設值要顯示空白
            string result = "";
            string c = parameter as string;
            if (c.Contains("N") && (r == DateTime.MinValue))
                result = "";
            else if (c.StartsWith("0"))
                result = r.ToString("yyyy/M/d");
            else if (c.StartsWith("1"))
                result = r.ToString("yyyy/M/d HH:mm:ss");

            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
