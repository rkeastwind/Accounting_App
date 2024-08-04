using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class ProDate : TableBaseDTO
    {
        public override string Table => "pro_date";

        [PrimaryKey, TableColumn, ColType(TableColTypeS.Date)]
        public DateTime pro_dt { get; set; }

        [PrimaryKey, TableColumn]
        public int pro_status { get; set; }

        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }
    }
}
