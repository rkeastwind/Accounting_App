using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.ValueConverters
{
    public class Vcvt_BasDept : ValueConverterBase<Vcvt_BasDept>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string r = value.NullToString();
            string col = parameter as string;
            string result = "";
            result = DBService.GetBasDept(r).dept_name;
            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
