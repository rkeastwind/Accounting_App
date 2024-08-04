using Accounting_App.DTO.BaseDTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class InvMast : TableBaseDTO
    {
        public override string Table => "inv_mast";

        [PrimaryKey, TableColumn]
        public string acct_book { get; set; }

        [PrimaryKey, TableColumn, ColType(TableColTypeS.Date)]
        public DateTime trade_dt { get; set; }

        [TableColumn]
        public decimal amt { get; set; }

        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }

        public string acct_book_name
        {
            get { return DBService.QryBookBase($@"where book = '{acct_book}'").FirstOrDefault().book_name; }
        }
    }
}
