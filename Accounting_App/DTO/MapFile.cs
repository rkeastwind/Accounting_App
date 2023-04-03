using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class MapFile
    {
        public string opt_no { get; set; }
        public string opt_name { get; set; }
        public string item { get; set; }
        public string item_name { get; set; }
        public string memo1 { get; set; }
        public string memo2 { get; set; }

        public Int32 order_by { get; set; }

        public string display_name
        {
            get { return item + " " + item_name; }
        }
    }
}
