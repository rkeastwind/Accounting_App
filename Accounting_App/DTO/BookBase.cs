using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.DTO
{
    public class BookBase
    {
        public string book { get; set; }
        public string book_name { get; set; }
        public string book_type { get; set; }
        public string bank { get; set; }
        public string bank_name { get; set; }
        public string account { get; set; }
        public string title { get; set; }

        public string display_name
        {
            get { return book + " " + book_name; }
        }
    }
}
