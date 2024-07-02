using Accounting_App.DTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.ValueConverters
{
    public class Vcvt_BookBase : ValueConverterBase<Vcvt_BookBase>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string r = value.NullToString();
            string col = parameter as string;
            string result = "";
            result = DBService.GetBookBase(true).Where(x => x.book == r).Select(s => s.book_name).FirstOrDefault();
            return result;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
