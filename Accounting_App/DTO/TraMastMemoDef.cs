using Accounting_App.DTO.BaseDTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class TraMastMemoDef : TableBaseDTO
    {
        public override string Table => "tra_mast_memodef";

        [PrimaryKey, TableColumn]
        public string action { get; set; }

        [PrimaryKey, TableColumn]
        public string action_dtl { get; set; }

        [PrimaryKey, TableColumn]
        public string acct_code { get; set; }

        [PrimaryKey, TableColumn]
        public string acct_book_in { get; set; }

        [PrimaryKey, TableColumn]
        public string acct_book_out { get; set; }

        [TableColumn]
        public string memodef { get; set; }

        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }

        public string loguserName
        {
            get
            {
                return CommUtility.GetUserName(loguser);
            }
        }
    }
}
