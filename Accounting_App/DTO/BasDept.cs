using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BasDept : TableBaseDTO
    {
        public override string Table => "bas_dept";
        [PrimaryKey, TableColumn]
        public string dept_id { get; set; }
        [TableColumn]
        public string dept_name { get; set; }
    }
}
