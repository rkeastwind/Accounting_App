using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class MapFile : TableBaseDTO
    {
        public override string Table => "map_file";

        [PrimaryKey, TableColumn]
        public string opt_no { get; set; }

        [TableColumn]
        public string opt_name { get; set; }

        [PrimaryKey, TableColumn]
        public string item { get; set; }

        [TableColumn]
        public string item_name { get; set; }

        [TableColumn]
        public string memo1 { get; set; }

        [TableColumn]
        public string memo2 { get; set; }

        [TableColumn]
        public int order_by { get; set; }

        public string display_name
        {
            get { return item + " " + item_name; }
        }
    }
}
