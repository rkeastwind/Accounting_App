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
            string qdt = dt.GetFullDate();
            DateTime _qdt = dt.AddMonths(1);
            string next_dt = new DateTime(_qdt.Year, _qdt.Month, DateTime.DaysInMonth(_qdt.Year, _qdt.Month)).GetFullDate();  //下個月底日

            if (direction == 1)  //正向
            {
                //前置作業，刪除庫存
                DBService.SQL_Command($"delete from inv_mast where date(trade_dt) >= '{qdt}'");

                List<InvMast> Bal = DBService.GetProBaseBal(dt);  //上個月底庫存(完整清單，沒有庫存的放0)
                List<TraMast> Tra = DBService.QryTraMast($"where date(trade_dt) between date('{qdt}','start of month') and '{qdt}'");  //月初到月底交易

                DateTime c_mbeg = dt.AddDays(-dt.Day + 1);  //月初

                while (c_mbeg <= dt)
                {
                    string qc_mbeg = c_mbeg.GetFullDate();

                    foreach (var b in Bal)
                    {
                        decimal amt_P = Tra.Where(x => x.acct_book_in == b.acct_book && x.trade_dt == c_mbeg).Sum(x => x.amt);
                        decimal amt_M = Tra.Where(x => x.acct_book_out == b.acct_book && x.trade_dt == c_mbeg).Sum(x => x.amt);
                        b.amt = b.amt + amt_P - amt_M;
                        b.trade_dt = c_mbeg;

                        if (b.amt < 0)
                        {
                            MessageBox.Show($"[{b.acct_book}_{b.acct_book_name}]於{qc_mbeg}結帳時發生庫存小於0 (${b.amt})，請檢查交易", "結帳失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return false;
                        }
                    }
                    c_mbeg = c_mbeg.AddDays(1);
                }

                //新增總帳
                Bal.Add(new InvMast()
                {
                    acct_book = "Total",
                    trade_dt = dt,
                    amt = Bal.Sum(x => x.amt)
                });
                //刪除0庫存
                Bal.RemoveAll(x => x.amt == 0);
                //寫入庫存
                foreach (var b in Bal)
                {
                    b.loguser = AppVar.User.user_id;
                    b.logtime = DateTime.Now;
                    b.InsertDB();
                }

                //更新狀態，並新增下次的結帳期初，先刪除後新增就不用判斷是否有資料，
                DBService.SQL_Command($"delete from pro_date where date(pro_dt) between '{qdt}' and '{next_dt}'");
                ProDate pd = new ProDate()
                {
                    pro_dt = Convert.ToDateTime(qdt),
                    pro_status = 1,
                    loguser = AppVar.User.user_id,
                    logtime = DateTime.Now
                };
                pd.InsertDB();
                pd.pro_dt = Convert.ToDateTime(next_dt);
                pd.pro_status = 0;
                pd.InsertDB();
            }
            else  //反向
            {
                DBService.SQL_Command($"update pro_date set pro_status = 0 where date(pro_dt) >= '{qdt}'");  //還原狀態，之前的全部清除
            }

            return true;
        }
    }
}
