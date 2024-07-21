using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accounting_App.DTO;
using Accounting_App.DTO.BaseDTO;
using System.Reflection;
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

        /// <summary>
        /// 取得使用者，呼叫端如有密碼要記得檢核不可為空
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static BasUser GetBasUser(string userid, string password = null)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = null;
                string statement = $@"select * from bas_user where 1=1 ";
                if (!string.IsNullOrEmpty(userid))
                    statement += "and user_id = @UserId ";
                if (!string.IsNullOrEmpty(password))
                    statement += "and password = @Password ";
                parameters = new { UserId = userid, Password = password };

                var result = conn.Query<BasUser>(statement, parameters);
                return result.ToList().FirstOrDefault();
            }
        }

        public static List<BasUser> QryBasUser(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string statement = $@"select * from bas_user ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<BasUser>(statement).OrderBy(x => x.user_id);
                return result.ToList();
            }
        }

        public static List<BasMenu> GetBasMenu(string roleid)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = null;
                string statement = $@"
SELECT a.*
FROM bas_menu a
INNER JOIN bas_role_permission b ON a.menu_id = b.menu_id
WHERE b.role_id = @RoleId
ORDER BY parent_id
";
                parameters = new { RoleId = roleid };

                var result = conn.Query<BasMenu>(statement, parameters);
                return result.ToList();
            }
        }

        public static BasRolePermission GetBasRolePermission(string roleid, string menuid)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = null;
                string statement = $@"select * from bas_role_permission where 1=1 ";
                if (!string.IsNullOrEmpty(roleid) && !string.IsNullOrEmpty(menuid))
                {
                    statement += "and role_id = @Roleid and menu_id = @Menuid";
                    parameters = new { RoleId = roleid, Menuid = menuid };
                }

                var result = conn.Query<BasRolePermission>(statement, parameters);
                return result.ToList().FirstOrDefault();
            }
        }

        public static BasDept GetBasDept(string deptid)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = null;
                string statement = $@"select * from bas_dept where 1=1 ";
                if (!string.IsNullOrEmpty(deptid))
                {
                    statement += "and dept_id = @DeptId ";
                    parameters = new { DeptId = deptid };
                }

                var result = conn.Query<BasDept>(statement, parameters);
                return result.ToList().FirstOrDefault();
            }
        }

        public static List<BasDept> QryBasDept(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string statement = $@"select * from bas_dept ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<BasDept>(statement).OrderBy(x => x.dept_id);
                return result.ToList();
            }
        }

        public static BasRole GetBasRole(string roleid)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                object parameters = null;
                string statement = $@"select * from bas_role where 1=1 ";
                if (!string.IsNullOrEmpty(roleid))
                {
                    statement += "and role_id = @RoleId ";
                    parameters = new { RoleId = roleid };
                }

                var result = conn.Query<BasRole>(statement, parameters);
                return result.ToList().FirstOrDefault();
            }
        }

        public static List<BasRole> QryBasRole(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string statement = $@"select * from bas_role ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<BasRole>(statement).OrderBy(x => x.role_id);
                return result.ToList();
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

        /// <summary>
        /// 取得BookBase
        /// </summary>
        /// <param name="InCludeTotal">是否含總帳</param>
        /// <returns></returns>
        public static List<BookBase> GetBookBase(bool InCludeTotal = false)
        {
            string statement = (InCludeTotal == false) ? "where book_type != 0" : "";
            var result = QryBookBase(statement);
            return result;
        }

        /// <summary>
        /// 取得book_base
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static List<BookBase> QryBookBase(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                string statement = $@"select * from book_base ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<BookBase>(statement).OrderBy(x => x.book_type).ThenBy(y => y.book);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得tra_mast
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static List<TraMast> QryTraMast(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from tra_mast ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<TraMast>(statement).OrderBy(x => x.trade_no);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得tra_mast_memodef
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static List<TraMastMemoDef> QryTraMastMemoDef(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from tra_mast_memodef ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<TraMastMemoDef>(statement)
                    .OrderBy(x => x.action).ThenBy(x => x.action_dtl).ThenBy(x => x.acct_code).ThenBy(x => x.acct_book_in).ThenBy(x => x.acct_book_out);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得inv_mast
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static List<InvMast> QryInvMast(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from inv_mast ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<InvMast>(statement);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得pro_date
        /// </summary>
        /// <param name="filter">篩選條件</param>
        /// <returns></returns>
        public static List<ProDate> QryProDate(string filter = "")
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DataTable table = new DataTable("MyTable");
                string statement = $@"select * from pro_date ";
                if (filter != "")
                    statement += filter;

                var result = conn.Query<ProDate>(statement);
                return result.ToList();
            }
        }

        /// <summary>
        /// 取得結帳庫存基底InvMast
        /// </summary>
        /// <param name="idt"></param>
        /// <returns></returns>
        public static List<InvMast> GetProBaseBal(DateTime idt)
        {
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                DateTime dt = idt.AddDays(-idt.Day);  //上個月底
                string sql = $@"
SELECT
    A.book AS acct_book,
    '{dt.GetFullDate()}' AS trade_dt,
    ifnull(B.amt,0) AS amt
FROM book_base A
left join (select * from inv_mast where date(trade_dt) = '{dt.GetFullDate()}') B ON A.book = B.acct_book
where A.book_type != 0 --排除總帳
";
                var result = conn.Query<InvMast>(sql).OrderBy(x => x.acct_book);
                return result.ToList();
            }
        }

        public static int InsertDB<T>(this T r) where T : TableBaseDTO
        {
            int affected = 0;
            if (r.GetType() != typeof(T))
                throw new Exception("寫入資料庫與傳入格式不符");

            var SqlScript = @"insert into [$Table] ([$Cols]) values ([$Values])";
            //處理值
            List<string> pkey = new List<string>();
            List<string> cols = new List<string>();
            var parameters = new DynamicParameters();
            foreach (var pi in r.GetType().GetProperties())
            {
                object vl = pi.GetValue(r, null);
                SetColByAttr(pi, vl, ref pkey, ref cols, ref parameters);
            }
            SqlScript = SqlScript.Replace("[$Table]", r.Table);
            SqlScript = SqlScript.Replace("[$Cols]", string.Join(", ", cols.ToArray()));
            SqlScript = SqlScript.Replace("[$Values]", string.Join(", ", cols.Select(x => "@" + x).ToArray()));
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                affected = conn.Execute(SqlScript, parameters);
            }
            return affected;
        }

        public static int UpdateDB<T>(this T r) where T : TableBaseDTO
        {
            int affected = 0;
            if (r.GetType() != typeof(T))
                throw new Exception("寫入資料庫與傳入格式不符");

            var SqlScript = @"update [$Table] set [$ColVal] where [$Key]";
            //處理值
            List<string> pkey = new List<string>();
            List<string> cols = new List<string>();
            var parameters = new DynamicParameters();
            foreach (var pi in r.GetType().GetProperties())
            {
                object vl = pi.GetValue(r, null);
                SetColByAttr(pi, vl, ref pkey, ref cols, ref parameters);
            }
            SqlScript = SqlScript.Replace("[$Table]", r.Table);
            SqlScript = SqlScript.Replace("[$ColVal]", string.Join(", ", cols.Where(x => !pkey.Contains(x)).Select(x => $@"{x} = @{x}").ToArray()));
            SqlScript = SqlScript.Replace("[$Key]", string.Join(" and ", pkey.Select(x => $@"{x} = @{x}").ToArray()));
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                affected = conn.Execute(SqlScript, parameters);
            }
            return affected;
        }

        public static int DeleteDB<T>(this T r) where T : TableBaseDTO
        {
            int affected = 0;
            if (r.GetType() != typeof(T))
                throw new Exception("寫入資料庫與傳入格式不符");

            var SqlScript = @"delete from [$Table] where [$Key]";
            //處理值
            List<string> pkey = new List<string>();
            List<string> cols = new List<string>();
            var parameters = new DynamicParameters();
            foreach (var pi in r.GetType().GetProperties())
            {
                object vl = pi.GetValue(r, null);
                SetColByAttr(pi, vl, ref pkey, ref cols, ref parameters);
            }
            SqlScript = SqlScript.Replace("[$Table]", r.Table);
            SqlScript = SqlScript.Replace("[$Key]", string.Join(" and ", pkey.Select(x => $@"{x} = @{x}").ToArray()));
            using (SQLiteConnection conn = new SQLiteConnection(cnStr))
            {
                affected = conn.Execute(SqlScript, parameters);
            }
            return affected;
        }


        private static void SetColByAttr(PropertyInfo pi, object vl, ref List<string> pkey, ref List<string> cols, ref DynamicParameters para)
        {
            //處理Attribute
            bool IsTableCol = false;
            var coltp = TableColTypeS.None;
            foreach (object attr in pi.GetCustomAttributes(true))
            {
                if ((attr as PrimaryKeyAttribute) != null)
                    pkey.Add(pi.Name);
                if ((attr as TableColumnAttribute) != null)
                {
                    cols.Add(pi.Name);
                    IsTableCol = true;
                }
                if ((attr as ColTypeAttribute) != null)
                    coltp = (attr as ColTypeAttribute).ColType;
            }
            if (!IsTableCol) return;   //不為TableCol

            //處理欄位值
            List<Type> DtTp = new List<Type>() {
                typeof(DateTime), typeof(DateTime?)
            };
            List<Type> NmTp = new List<Type>() {
                typeof(decimal), typeof(double), typeof(float), typeof(int),
                typeof(decimal?), typeof(double?), typeof(float?), typeof(int?)
            };
            //預設DB不可Null，日期轉TEXT(SQLLite沒有日期格式)
            if (DtTp.Contains(pi.PropertyType))  //日期
            {
                DateTime dt = (vl == null) ? new DateTime(1900, 01, 01) : Convert.ToDateTime(vl);
                if (coltp == TableColTypeS.Date)
                    vl = dt.GetFullDate();
                else
                    vl = dt.GetFullDateTime();
            }
            else if (NmTp.Contains(pi.PropertyType))  //數字
                vl = (vl == null) ? 0 : vl;
            else  //其他
                vl = (vl == null) ? "" : vl;

            para.Add(pi.Name, vl);
        }
    }
}
