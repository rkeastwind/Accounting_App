using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting_App.DTO;

namespace Accounting_App.Utilities
{
    public static class CommUtility
    {

        /// <summary>
        /// 填充空白選項
        /// </summary>
        /// <param name="I_MapFile"></param>
        /// <returns></returns>
        public static List<MapFile> InsertBlankItem(List<MapFile> I_MapFile)
        {
            List<MapFile> MF = new List<MapFile>();
            MF.AddRange(I_MapFile);
            MapFile A = MF.Find(x => x.item == "");
            if (A == null)
            {
                MF.Add(new MapFile()
                {
                    opt_no = MF.FirstOrDefault().opt_no,
                    opt_name = MF.FirstOrDefault().opt_name,
                    item = "",
                    item_name = "",
                    memo1 = "",
                    memo2 = ""
                });
            }
            return MF.OrderBy(x => x.item).ToList();
        }


        /// <summary>
        /// 取得下一個TraMast交易編號
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string GetNextTradeNo_TraMast(DateTime dt)
        {
            string trade_no = "";
            string leading_code = dt.ToString("yyyyMMdd");
            DataRow rw = DBService.QryTraMast($@"where trade_no like '{leading_code}%' order by trade_no desc").AsEnumerable().FirstOrDefault();
            if (rw != null)
            {
                string max_trade_no = rw["trade_no"].ToString();
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
        /// 判斷當月是否結帳
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool CheckIsPro(DateTime dt)
        {
            string Month = dt.ToString("MM");
            DataTable Qry = DBService.QryProDate($@"where strftime('%m', pro_dt) = '{Month}' and pro_status = 1");
            if (Qry.Rows.Count > 0)
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
            DataRow rw = DBService.QryProDate("where pro_dt = (select MAX(pro_dt) from pro_date where pro_status = 1)").AsEnumerable().FirstOrDefault();
            if (rw != null)
            {
                return rw.Field<DateTime>("pro_dt").AddDays(1);  //最大結帳日+1
            }
            else
            {
                return DateTime.Today;
            }
        }

        /// <summary>
        /// 將DataTable轉換成CSV文件
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="filePath">文件路徑</param>
        public static string SaveCSV(DataTable dt)
        {
            string sw = "";
            string data = "";

            //寫出列名稱
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                data += dt.Columns[i].ColumnName.ToString();
                if (i < dt.Columns.Count - 1)
                {
                    data += ",";
                }
            }
            sw += data + "\r\n";

            //寫出各行數據
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                data = "";
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    data += dt.Rows[i][j].ToString();
                    if (j < dt.Columns.Count - 1)
                    {
                        data += ",";
                    }
                }
                sw += data + "\r\n";
            }
            return sw;
        }
    }
}
