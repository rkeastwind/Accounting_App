using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting_App.DTO;
using Dapper;

namespace Accounting_App.Utilities
{
    public static class DBService
    {
        private static string cnStr = "data source=" + ConfigurationManager.AppSettings["DBPath"];

        /// <summary>
        /// SQL_Command
        /// </summary>
        /// <param name="r"></param>
        public static int SQL_Command(string sql)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                return conn.Execute(sql);
            }
        }

        /// <summary>
        /// SQL_QryTable
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <param name="cvdt_column">轉日期格式欄位</param>
        /// <returns></returns>
        public static DataTable SQL_QryTable(string sql, string[] cvdt_column)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                var result = conn.ExecuteReader(sql);
                table.Load(result);
                if (cvdt_column.Count() != 0)  //轉日期
                {
                    table = ConvertColumnToDate(table, cvdt_column);
                }
                return table;
            }
        }

        /// <summary>
        /// 取得使用者資訊
        /// </summary>
        /// <returns>UserInfo物件</returns>
        public static UserInfo GetUserInfo()
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                UserInfo ui = new UserInfo();
                string strSql = "select opt_no, item from map_file where opt_no in ('User','Dept')";
                var result = conn.Query(strSql);  //動態型別
                foreach (var r in result)
                {
                    if (r.opt_no == "User") { ui.UserName = r.item; }
                    if (r.opt_no == "Dept") { ui.Department = r.item; }
                }
                return ui;
            }
        }

        /// <summary>
        /// 更新使用者資訊
        /// </summary>
        /// <param name="usif">UserInfo物件</param>
        /// <returns></returns>
        public static Boolean UpdUserInfo(UserInfo usif)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string strSql = "update map_file set item = @Ucol, item_name = @Ucol where opt_no = @Wcol";
                object cols = new[]
                {
                    new{ @Ucol = usif.UserName, @Wcol = "User"},
                    new{ @Ucol = usif.Department, @Wcol = "Dept"}
                };
                conn.Execute(strSql, cols);
                return true;
            }
        }

        /// <summary>
        /// 取得map_file
        /// </summary>
        /// <param name="opt_no">代碼</param>
        /// <param name="where_item">where條件用object</param>
        /// <returns></returns>
        public static List<MapFile> GetMapFile(string opt_no, object[] where_item = null)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = where_item;
                string statement = $@"select * from map_file where opt_no = '{opt_no}' ";
                if (where_item != null)
                {
                    statement += "and item in @Items";
                    parameters = new { Items = where_item };
                }

                var result = conn.Query<MapFile>(statement, parameters).OrderBy(x => x.order_by).ThenBy(x => x.item);
                return result.ToList();
            }
        }

        #region book_base

        /// <summary>
        /// 取得BookBase
        /// </summary>
        /// <param name="InCludeTotal">是否含總帳</param>
        /// <returns></returns>
        public static List<BookBase> GetBookBase(bool InCludeTotal = false)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string statement = $@"select * from book_base ";
                if (InCludeTotal == false)
                    statement += "where book_type != 0";

                var result = conn.Query<BookBase>(statement).OrderBy(x => x.book_type).ThenBy(y => y.book);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得book_base
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static DataTable QryBookBase(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from book_base ";
                if (filter != "")
                    statement += filter;

                var result = conn.ExecuteReader(statement + " order by book_type, book");
                table.Load(result);
                //table = ConvertColumnToDate(table, new string[] { "trade_dt", "logtime" });
                return table;
            }
        }

        /// <summary>
        /// Insert_book_base
        /// </summary>
        /// <param name="r"></param>
        public static void InsBookBase(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var insertScript = $@"insert into book_base values (" +
                    $@"'{r["book"]}'" + "," +
                    $@"'{r["book_name"]}'" + "," +
                    $@"'{r["book_type"]}'" + "," +
                    $@"'{r["bank"]}'" + "," +
                    $@"'{r["bank_name"]}'" + "," +
                    $@"'{r["account"]}'" + "," +
                    $@"'{r["title"]}'" + ")";
                conn.Execute(insertScript);
            }
        }

        /// <summary>
        /// Update_book_base
        /// </summary>
        /// <param name="r"></param>
        public static void UpdBookBase(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var updateScript = $@"update book_base set " +
                    $@"book_name = '{r["book_name"]}'" + "," +
                    $@"book_type = '{r["book_type"]}'" + "," +
                    $@"bank = '{r["bank"]}'" + "," +
                    $@"bank_name = '{r["bank_name"]}'" + "," +
                    $@"account = '{r["account"]}'" + "," +
                    $@"title = '{r["title"]}'" +
                    " where " +
                    $@"book = '{r["book"]}'";
                conn.Execute(updateScript);
            }
        }

        /// <summary>
        /// Delete_book_base
        /// </summary>
        /// <param name="r"></param>
        public static void DelBookBase(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var deleteScript = $@"delete from book_base where " +
                    $@"book = '{r["book"]}'";
                conn.Execute(deleteScript);
            }
        }

        #endregion book_base

        #region tra_mast

        /// <summary>
        /// 取得tra_mast
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static DataTable QryTraMast(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from tra_mast ";
                if (filter != "")
                    statement += filter;

                var result = conn.ExecuteReader(statement + " order by trade_no");
                table.Load(result);
                table = ConvertColumnToDate(table, new string[] { "trade_dt", "logtime" });
                return table;
            }
        }

        /// <summary>
        /// Insert_tra_mast
        /// </summary>
        /// <param name="r"></param>
        public static void InsTraMast(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var insertScript = $@"insert into tra_mast values (" +
                    $@"'{r["trade_no"]}'" + "," +
                    $@"'{((DateTime)r["trade_dt"]).ToString("yyyy-MM-dd")}'" + "," +
                    $@"'{r["action"]}'" + "," +
                    $@"'{r["action_dtl"]}'" + "," +
                    $@"'{r["acct_code"]}'" + "," +
                    $@"'{r["acct_book_in"]}'" + "," +
                    $@"'{r["acct_book_out"]}'" + "," +
                    $@"'{r["amt"]}'" + "," +
                    $@"'{r["memo"]}'" + "," +
                    $@"'{r["loguser"]}'" + "," +
                    $@"'{((DateTime)r["logtime"]).ToString("yyyy-MM-dd hh:mm:ss")}'" + ")";
                conn.Execute(insertScript);
            }
        }

        /// <summary>
        /// Update_tra_mast
        /// </summary>
        /// <param name="r"></param>
        public static void UpdTraMast(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var updateScript = $@"update tra_mast set " +
                    $@"trade_dt = '{((DateTime)r["trade_dt"]).ToString("yyyy-MM-dd")}'" + "," +
                    $@"action = '{r["action"]}'" + "," +
                    $@"action_dtl = '{r["action_dtl"]}'" + "," +
                    $@"acct_code = '{r["acct_code"]}'" + "," +
                    $@"acct_book_in = '{r["acct_book_in"]}'" + "," +
                    $@"acct_book_out = '{r["acct_book_out"]}'" + "," +
                    $@"amt = '{r["amt"]}'" + "," +
                    $@"memo = '{r["memo"]}'" + "," +
                    $@"loguser = '{r["loguser"]}'" + "," +
                    $@"logtime = '{((DateTime)r["logtime"]).ToString("yyyy-MM-dd hh:mm:ss")}'" +
                    " where " +
                    $@"trade_no = '{r["trade_no"]}'";
                conn.Execute(updateScript);
            }
        }

        /// <summary>
        /// Delete_tra_mast
        /// </summary>
        /// <param name="r"></param>
        public static void DelTraMast(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var deleteScript = $@"delete from tra_mast where " +
                    $@"trade_no = '{r["trade_no"]}'";
                conn.Execute(deleteScript);
            }
        }

        #endregion tra_mast

        #region tra_mast_memodef

        /// <summary>
        /// 取得tra_mast_memodef
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static DataTable QryTraMastMemoDef(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from tra_mast_memodef ";
                if (filter != "")
                    statement += filter;

                var result = conn.ExecuteReader(statement + " order by action, action_dtl, acct_code, acct_book_in, acct_book_out");
                table.Load(result);
                table = ConvertColumnToDate(table, new string[] { "logtime" });
                return table;
            }
        }

        /// <summary>
        /// Insert_mast_memodef
        /// </summary>
        /// <param name="r"></param>
        public static void InsTraMastMemoDef(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var insertScript = $@"insert into tra_mast_memodef values (" +
                    $@"'{r["action"]}'" + "," +
                    $@"'{r["action_dtl"]}'" + "," +
                    $@"'{r["acct_code"]}'" + "," +
                    $@"'{r["acct_book_in"]}'" + "," +
                    $@"'{r["acct_book_out"]}'" + "," +
                    $@"'{r["memodef"]}'" + "," +
                    $@"'{r["loguser"]}'" + "," +
                    $@"'{((DateTime)r["logtime"]).ToString("yyyy-MM-dd hh:mm:ss")}'" + ")";
                conn.Execute(insertScript);
            }
        }

        /// <summary>
        /// Update_tra_mast_memodef
        /// </summary>
        /// <param name="r"></param>
        public static void UpdTraMastMemoDef(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var updateScript = $@"update tra_mast_memodef set " +
                    $@"memodef = '{r["memodef"]}'" + "," +
                    $@"loguser = '{r["loguser"]}'" + "," +
                    $@"logtime = '{((DateTime)r["logtime"]).ToString("yyyy-MM-dd hh:mm:ss")}'" +
                    " where " +
                    $@"action = '{r["action"]}'" + " and " +
                    $@"action_dtl = '{r["action_dtl"]}'" + " and " +
                    $@"acct_code = '{r["acct_code"]}'" + " and " +
                    $@"acct_book_in = '{r["acct_book_in"]}'" + " and " +
                    $@"acct_book_out = '{r["acct_book_out"]}'";
                conn.Execute(updateScript);
            }
        }

        /// <summary>
        /// Delete_tra_mast_memodef
        /// </summary>
        /// <param name="r"></param>
        public static void DelTraMastMemoDef(DataRowView r)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var deleteScript = $@"delete from tra_mast_memodef where " +
                    $@"action = '{r["action"]}'" + " and " +
                    $@"action_dtl = '{r["action_dtl"]}'" + " and " +
                    $@"acct_code = '{r["acct_code"]}'" + " and " +
                    $@"acct_book_in = '{r["acct_book_in"]}'" + " and " +
                    $@"acct_book_out = '{r["acct_book_out"]}'";
                conn.Execute(deleteScript);
            }
        }

        #endregion tra_mast_memodef

        #region 結帳相關(inv_mast、pro_date)

        /// <summary>
        /// 取得inv_mast
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static DataTable QryInvMast(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from inv_mast ";
                if (filter != "")
                    statement += filter;

                var result = conn.ExecuteReader(statement);
                table.Load(result);
                table = ConvertColumnToDate(table, new string[] { "trade_dt", "logtime" });
                return table;
            }
        }

        /// <summary>
        /// Delete_inv_mast
        /// </summary>
        /// <param name="trade_dt"></param>
        public static void DelInvMast(DateTime trade_dt)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                //注意日期格式
                var deleteScript = $@"delete from inv_mast where " +
                    $@"trade_dt = '{trade_dt.ToString("yyyy-MM-dd")}'";
                conn.Execute(deleteScript);
            }
        }

        /// <summary>
        /// 取得pro_date
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static DataTable QryProDate(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from pro_date ";
                if (filter != "")
                    statement += filter;

                var result = conn.ExecuteReader(statement);
                table.Load(result);
                table = ConvertColumnToDate(table, new string[] { "pro_dt", "logtime" });
                return table;
            }
        }

        /// <summary>
        /// 取得結帳庫存基底ProBal
        /// </summary>
        /// <param name="idt"></param>
        /// <returns></returns>
        public static List<ProBal> GetProBal(DateTime idt)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DateTime dt = idt.AddDays(-idt.Day);  //上個月底
                string sql = $@"
SELECT
	A.book, A.book_name,
	'{dt.ToString("yyyy-MM-dd")}' AS trade_dt, ifnull(B.amt,0) AS amt
FROM book_base A
left join (select * from inv_mast where date(trade_dt) = '{dt.ToString("yyyy-MM-dd")}') B ON A.book = B.acct_book
where A.book_type != 0 --排除總帳
";

                var result = conn.Query<ProBal>(sql).OrderBy(x => x.book);
                return result.ToList();
            }
        }

        /// <summary>
        /// Insert_inv_mast
        /// </summary>
        /// <param name="p"></param>
        public static void InsInvMast(List<ProBal> p)
        {
            if (p.Count() == 0) return;
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string insertScript = $@"insert into inv_mast values ";
                foreach (var a in p)
                {
                    //注意日期格式
                    insertScript += "\r\n" + "(" +
                        $@"'{a.book}'" + "," +
                        $@"'{a.trade_dt.ToString("yyyy-MM-dd")}'" + "," +
                        $@"'{a.amt}'" + "," +
                        $@"'{AppVar.UserName}'" + "," +
                        $@"'{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")}'" + "),";
                }
                insertScript = insertScript.TrimEnd(',');
                conn.Execute(insertScript);
            }
        }

        #endregion 結帳相關(inv_mast、pro_date)

        /// <summary>
        /// 轉換Datatable欄位格式為DateTime(因為SQLite沒有DateTime)
        /// </summary>
        /// <param name="I_Dt">輸入table</param>
        /// <param name="I_Co">欄位名稱</param>
        /// <returns></returns>
        public static DataTable ConvertColumnToDate(DataTable I_Dt, string[] I_Co)
        {
            DataTable dtCloned = I_Dt.Clone();
            foreach (var c in I_Co)
            {
                dtCloned.Columns[c].DataType = typeof(DateTime);
            }
            foreach (DataRow row in I_Dt.Rows)
            {
                dtCloned.ImportRow(row);
            }
            return dtCloned;
        }
    }
}
