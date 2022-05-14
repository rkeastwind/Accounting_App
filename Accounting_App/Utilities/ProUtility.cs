using Accounting_App.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Accounting_App.Utilities
{
    public static class ProUtility
    {
        public static bool ExecutePro(DateTime dt, int direction)
        {
            string qdt = dt.ToString("yyyy-MM-dd");
            DateTime _qdt = dt.AddMonths(1);
            string next_dt = new DateTime(_qdt.Year, _qdt.Month, DateTime.DaysInMonth(_qdt.Year, _qdt.Month)).ToString("yyyy-MM-dd");  //下個月底日

            if (direction == 1)  //正向
            {
                //前置作業，刪除庫存
                DBService.SQL_Command($"delete from inv_mast where date(trade_dt) >= '{qdt}'");

                List<ProBal> Bal = DBService.GetProBal(dt);  //上個月底庫存(完整清單，沒有庫存的放0)
                DataTable Tra = DBService.QryTraMast($"where date(trade_dt) between date('{qdt}','start of month') and '{qdt}'");  //月初到月底交易

                DateTime c_mbeg = dt.AddDays(-dt.Day + 1);  //月初

                while (c_mbeg <= dt)
                {
                    string qc_mbeg = c_mbeg.ToString("yyyy-MM-dd");

                    foreach (var b in Bal)
                    {
                        object _amt_P = Tra.Compute("SUM(amt)", $"acct_book_in = '{b.book}' and trade_dt = '{qc_mbeg}'");
                        object _amt_M = Tra.Compute("SUM(amt)", $"acct_book_out = '{b.book}' and trade_dt = '{qc_mbeg}'");
                        decimal amt_P = _amt_P is decimal ? (decimal)_amt_P : 0;
                        decimal amt_M = _amt_M is decimal ? (decimal)_amt_M : 0;
                        b.amt = b.amt + amt_P - amt_M;
                        b.trade_dt = c_mbeg;

                        if (b.amt < 0)
                        {
                            MessageBox.Show($"[{b.book}_{b.book_name}]於{qc_mbeg}結帳時發生庫存小於0 (${b.amt})，請檢查交易", "結帳失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    c_mbeg = c_mbeg.AddDays(1);
                }

                //新增總帳
                Bal.Add(new ProBal()
                {
                    book = "Total",
                    book_name = "總帳",
                    trade_dt = dt,
                    amt = Bal.Sum(x => x.amt)
                });
                //刪除0庫存
                Bal.RemoveAll(x => x.amt == 0);
                //寫入庫存
                DBService.InsInvMast(Bal);

                //更新狀態，並新增下次的結帳期初，先刪除後新增就不用判斷是否有資料，
                DBService.SQL_Command($"delete from pro_date where date(pro_dt) between '{qdt}' and '{next_dt}'");
                DBService.SQL_Command($"insert into pro_date values('{qdt}',1,'{AppVar.UserName}','{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}')");
                DBService.SQL_Command($"insert into pro_date values('{next_dt}',0,'{AppVar.UserName}','{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}')");
            }
            else  //反向
            {
                DBService.SQL_Command($"update pro_date set pro_status = 0 where date(pro_dt) >= '{qdt}'");  //還原狀態，之前的全部清除
            }

            return true;
        }
    }
}
