using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BasRolePermission : TableBaseDTO
    {
        public override string Table => "bas_role_permission";
        [PrimaryKey, TableColumn]
        public string role_id { get; set; }
        [PrimaryKey, TableColumn]
        public string menu_id { get; set; }
        [TableColumn]
        public bool cmd_Qurey { get; set; }
        [TableColumn]
        public bool cmd_Add { get; set; }
        [TableColumn]
        public bool cmd_Edit { get; set; }
        [TableColumn]
        public bool cmd_Delete { get; set; }
        [TableColumn]
        public bool cmd_Export { get; set; }
    }
}
