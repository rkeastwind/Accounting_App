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
        public static string UserName;
        public static string Department;

        public static void RefreshUser()
        {
            UserInfo userInfo = DBService.GetUserInfo();
            UserName = userInfo.UserName;
            Department = userInfo.Department;
        }
    }
}
