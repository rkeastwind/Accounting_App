using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BasRole : TableBaseDTO
    {
        public override string Table => "bas_role";
        [PrimaryKey, TableColumn]
        public string role_id { get; set; }
        [TableColumn]
        public string role_name { get; set; }
    }
}
