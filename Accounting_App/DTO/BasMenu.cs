using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BasMenu : TableBaseDTO
    {
        public override string Table => "bas_menu";
        [PrimaryKey, TableColumn]
        public string menu_id { get; set; }
        [TableColumn]
        public string parent_id { get; set; }
        [TableColumn]
        public int level { get; set; }
        [TableColumn]
        public int order_by { get; set; }
        [TableColumn]
        public string menu_type { get; set; }
        [TableColumn]
        public string menu_title { get; set; }
        [TableColumn]
        public string icon_path { get; set; }
        [TableColumn]
        public string page_url { get; set; }
        [TableColumn]
        public string page_para { get; set; }

        public List<BasMenu> child;
    }
}
