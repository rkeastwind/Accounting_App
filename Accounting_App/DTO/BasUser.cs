using Accounting_App.DTO.BaseDTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BasUser : TableBaseDTO
    {
        public override string Table => "bas_user";
        [PrimaryKey, TableColumn]
        public string user_id { get; set; }
        [TableColumn]
        public string name { get; set; }
        [TableColumn]
        public string password { get; set; }
        [TableColumn]
        public string role_id { get; set; }
        [TableColumn]
        public string dept_id { get; set; }
        [TableColumn]
        public bool enabled { get; set; }
        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }

        public string dept_name
        {
            get
            {
                var r = DBService.GetBasDept(dept_id);
                return (r == null) ? null : r.dept_name;
            }
        }

        public string role_name
        {
            get
            {
                var r = DBService.GetBasRole(role_id);
                return (r == null) ? null : r.role_name;
            }
        }
    }
}
