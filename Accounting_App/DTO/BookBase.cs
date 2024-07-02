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

        public string display_name
        {
            get { return book + " " + book_name; }
        }
    }
}
