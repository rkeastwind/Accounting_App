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
