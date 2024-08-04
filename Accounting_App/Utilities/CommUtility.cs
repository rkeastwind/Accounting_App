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
                    memo2 = "",
                    order_by = 0
                });
            }
            return MF.OrderBy(x => x.order_by).ThenBy(x => x.item).ToList();
        }

        /// <summary>
        /// 填充空白選項
        /// </summary>
        /// <param name="I_BookBase"></param>
        /// <returns></returns>
        public static List<BookBase> InsertBlankItem(List<BookBase> I_BookBase)
        {
            List<BookBase> MF = new List<BookBase>();
            MF.AddRange(I_BookBase);
            BookBase A = MF.Find(x => x.book == "");
            if (A == null)
            {
                MF.Add(new BookBase()
                {
                    book = "",
                    book_name = "",
                    bank = "",
                    bank_name = "",
                    account = "",
                    title = ""
                });
            }
            return MF.OrderBy(x => x.book).ToList();
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
