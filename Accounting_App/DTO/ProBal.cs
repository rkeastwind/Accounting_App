using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class ProBal
    {
        public string book { get; set; }
        public string book_name { get; set; }
        public DateTime trade_dt { get; set; }

        public decimal amt { get; set; }
    }
}
