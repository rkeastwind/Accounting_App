using Accounting_App.DTO.BaseDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting_App.Utilities;

namespace Accounting_App.DTO
{
    public class BookBase : TableBaseDTO
    {
        public override string Table => "book_base";

        [PrimaryKey, TableColumn]
        public string book { get; set; }

        [TableColumn]
        public string book_name { get; set; }

        [TableColumn]
        public string book_type { get; set; }

        [TableColumn]
        public string bank { get; set; }

        [TableColumn]
        public string bank_name { get; set; }

        [TableColumn]
        public string account { get; set; }

        [TableColumn]
        public string title { get; set; }

        [TableColumn, ColType(TableColTypeS.Date)]
        public DateTime? open_date { get; set; }  //紀錄用，目前沒有使用

        /* 關閉帳冊日期為[上次結帳日]：例.上次結帳日2/28庫存為0，本次結帳日為3/31做關閉帳冊，日期會壓2/28
         * 關閉帳冊檢核：上次結帳日庫存為0或無庫存，且大於上次結帳日沒有任何交易(進/出)
         * 開啟帳冊檢核：close_date必須大於等於上次結帳日，即關閉帳冊的時間點開始才能解鎖
         */
        [TableColumn, ColType(TableColTypeS.Date)]
        public DateTime? close_date { get; set; }

        [TableColumn]
        public bool in_qurey { get; set; }  //是否顯示於報表查詢選單，USER可以控制帳冊關閉後，是否可以查歷史資料

        [TableColumn]
        public string loguser { get; set; }

        [TableColumn, ColType(TableColTypeS.DateTime)]
        public DateTime? logtime { get; set; }

        public string display_name
        {
            get { return book + " " + book_name; }
        }

        public bool Is_Closed
        {
            get { return (close_date.HasValue); }
        }
    }
}
