﻿using Accounting_App.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.Utilities
{
    public static class ProUtility
    {
        /// <summary>
        /// 取得下一個TraMast交易編號
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetNextTradeNo_TraMast(DateTime dt)
        {
            string trade_no = "";
            string leading_code = dt.ToString("yyyyMMdd");
            TraMast rw = DBService.QryTraMast($@"where trade_no like '{leading_code}%'").AsEnumerable().OrderByDescending(x => x.trade_no).FirstOrDefault();
            if (rw != null)
            {
                string max_trade_no = rw.trade_no;
                int new_no = int.Parse(max_trade_no.Replace(leading_code, "")) + 1;
                trade_no = leading_code + new_no.ToString().PadLeft(5, '0');
            }
            else
            {
                trade_no = leading_code + "1".PadLeft(5, '0');
            }
            return trade_no;
        }

        /// <summary>
        /// 判斷當年月是否結帳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool CheckIsPro(DateTime dt)
        {
            string YearMonth = dt.ToString("yyyy-MM");
            var Qry = DBService.QryProDate($@"where strftime('%Y-%m', pro_dt) = '{YearMonth}' and pro_status = 1");
            if (Qry.Count > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判斷帳冊是否已結帳
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static bool CheckBookIsPro(string book)
        {
            DataTable Qry = DBService.SQL_QryTable($@"
select *
from pro_date A
left join inv_mast B on A.pro_dt = B.trade_dt
where acct_book = '{book}' and pro_status = 1
", new string[] { });
            if (Qry.Rows.Count > 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 取得下一結帳起始日
        /// </summary>
        /// <returns></returns>
        public static DateTime GetNextProStartDt()
        {
            var rw = DBService.QryProDate("where pro_dt = (select MAX(pro_dt) from pro_date where pro_status = 1)").FirstOrDefault();
            return (rw != null) ? rw.pro_dt.AddDays(1) : DateTime.Today;  //最大結帳日+1
        }

        /// <summary>
        /// 取得下一結帳迄日
        /// </summary>
        /// <returns></returns>
        public static DateTime GetNextProEndDt()
        {
            var rw = DBService.QryProDate("where pro_dt = (select MIN(pro_dt) from pro_date where pro_status = 0)").FirstOrDefault();
            return (rw != null) ? rw.pro_dt : DateTime.Today;
        }

        /// <summary>
        /// 取得上一結帳迄日
        /// </summary>
        /// <returns></returns>
        public static DateTime GetLastProEndDt()
        {
            var rw = DBService.QryProDate("where pro_dt = (select MAX(pro_dt) from pro_date where pro_status = 1)").FirstOrDefault();
            return (rw != null) ? rw.pro_dt : DateTime.Today;
        }

        /// <summary>
        /// 找備註預設值(欄位吻合最多的那筆)
        /// </summary>
        /// <param name="action"></param>
        /// <param name="action_dtl"></param>
        /// <param name="acct_code"></param>
        /// <param name="acct_book_in"></param>
        /// <param name="acct_book_out"></param>
        /// <returns></returns>
        public static string GetTraMastMemoDef(string action, string action_dtl, string acct_code, string acct_book_in, string acct_book_out)
        {
            string result = "";
            if (string.IsNullOrWhiteSpace(action) && string.IsNullOrWhiteSpace(action_dtl) && string.IsNullOrWhiteSpace(acct_code) &&
                string.IsNullOrWhiteSpace(acct_book_in) && string.IsNullOrWhiteSpace(acct_book_out))
                return result;

            string QryStr = @"
select
[CtStr]
,*
from tra_mast_memodef
[WhStr]
order by cnt desc
";
            //吻合計數(找最高的)
            string CtStr = " 0";
            CtStr += (string.IsNullOrWhiteSpace(action)) ? "" : $"\r\n + case when action = '{action}' then 1 else 0 end";
            CtStr += (string.IsNullOrWhiteSpace(action_dtl)) ? "" : $"\r\n + case when action_dtl = '{action_dtl}' then 1 else 0 end";
            CtStr += (string.IsNullOrWhiteSpace(acct_code)) ? "" : $"\r\n + case when acct_code = '{acct_code}' then 1 else 0 end";
            CtStr += (string.IsNullOrWhiteSpace(acct_book_in)) ? "" : $"\r\n + case when acct_book_in = '{acct_book_in}' then 1 else 0 end";
            CtStr += (string.IsNullOrWhiteSpace(acct_book_out)) ? "" : $"\r\n + case when acct_book_out = '{acct_book_out}' then 1 else 0 end";
            CtStr += "\r\n as cnt";

            //來源有值代表要找設定是ALL和一樣的那一列
            string WhStr = "where 1=1";
            WhStr += (string.IsNullOrWhiteSpace(action)) ? "" : $"\r\n and (action = '' or action = '{action}')";
            WhStr += (string.IsNullOrWhiteSpace(action_dtl)) ? "" : $"\r\n and (action_dtl = '' or action_dtl = '{action_dtl}')";
            WhStr += (string.IsNullOrWhiteSpace(acct_code)) ? "" : $"\r\n and (acct_code = '' or acct_code = '{acct_code}')";
            WhStr += (string.IsNullOrWhiteSpace(acct_book_in)) ? "" : $"\r\n and (acct_book_in = '' or acct_book_in = '{acct_book_in}')";
            WhStr += (string.IsNullOrWhiteSpace(acct_book_out)) ? "" : $"\r\n and (acct_book_out = '' or acct_book_out = '{acct_book_out}')";

            QryStr = QryStr.Replace("[CtStr]", CtStr).Replace("[WhStr]", WhStr);
            DataTable dt = DBService.SQL_QryTable(QryStr, new string[] { });
            if (dt.Rows.Count == 0) return result;

            return dt.Rows[0]["memodef"].ToString();
        }
    }
}
