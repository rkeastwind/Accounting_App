using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class TraMast : TableBaseDTO
    {
        public override string Table => "tra_mast";

        [PrimaryKey, TableColumn]
        public string trade_no { get; set; }

        [TableColumn, ColType(TableColTypeS.Date)]
        public DateTime trade_dt { get; set; }

        [TableColumn]
        public string action { get; set; }

        [TableColumn]
        public string action_dtl { get; set; }

        [TableColumn]
        public string acct_code { get; set; }

        [TableColumn]
        public string acct_book_in { get; set; }

        [TableColumn]
        public string acct_book_out { get; set; }

        [TableColumn]
        public decimal amt { get; set; }

        [TableColumn]
        public string memo { get; set; }

        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }
    }
}
