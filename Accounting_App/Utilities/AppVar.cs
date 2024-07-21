using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting_App.DTO;

namespace Accounting_App.Utilities
{
    public static class AppVar
    {
        /// <summary>
        /// 最小日期1900-01-01
        /// </summary>
        public static DateTime DtMinValue = new DateTime(1900, 01, 01);
        public static BasUser User;
        public static string OpenMenuId;  //本次開啟的MenuId

        public static void SetUserInfo(string user_id)
        {
            User = DBService.GetBasUser(user_id);
        }
    }
}
