using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.IO;
using System.Data;
using Accounting_App.Utilities;
using Excel = Microsoft.Office.Interop.Excel;
using DataTable = System.Data.DataTable;
using System.Windows.Input;
using Accounting_App.DTO;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using Accounting_App.UserControls;

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_rpt_R002.xaml 的互動邏輯
    /// </summary>
    public partial class Form_rpt_R002 : Window
    {
        public Form_rpt_R002()
        {
            InitializeComponent();
            BindBookBase();
        }

        private void BindBookBase()
        {
            DG_Main.ItemsSource = DBService.GetBookBaseForQry(true);  //庫存表開放查詢總帳冊
            DG_Main.SelectionChanged += Checked_SelectionChanged;
            CheckBox_Checked(dgc_check_header, null);
        }

        private bool _handle = true;  //預設true等於開啟畫面全選的意思
        private void Checked_SelectionChanged(object sender, RoutedEventArgs e)
        {
            _handle = false;
            dgc_check_header.IsChecked = (DG_Main.SelectedItems.Count == DG_Main.Items.Count) ? true : false;
            _handle = true;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_handle)
                DG_Main.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_handle)
                DG_Main.SelectedItems.Clear();
        }

        private List<BookBase> GetSelectedBook()
        {
            List<BookBase> book = new List<BookBase>();
            foreach (BookBase r in DG_Main.SelectedItems)
            {
                book.Add(r);
            }
            return book;
        }


        private bool CheckSelectDate()
        {
            if (DG_Main.SelectedItems.Count == 0)
            {
                new MessageBoxCustom("請選擇帳冊", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                return false;
            }
            if (Qry_Beg_Dt.SelectedDate == null || Qry_End_Dt.SelectedDate == null)
            {
                new MessageBoxCustom("查詢日期不可空白", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                return false;
            }
            DateTime QryDtBeg = (DateTime)Qry_Beg_Dt.SelectedDate;
            DateTime QryDtEnd = (DateTime)Qry_End_Dt.SelectedDate;
            if (QryDtBeg.Year > QryDtEnd.Year || (QryDtBeg.Year == QryDtEnd.Year && QryDtBeg.Month > QryDtEnd.Month))  //年份大於，或年份相同但月份大於
            {
                new MessageBoxCustom("起日不可大於迄日", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                return false;
            }
            return true;
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            if (CheckSelectDate() == false)
                return;

            string localFilePath = "";
            string fileNameExt = "";

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.RestoreDirectory = true;  //儲存對話方塊是否記憶上次開啟的目錄 
            dialog.Title = "請選擇資料夾";
            dialog.Filter = "Excel(*.xlsx)|*.xlsx";
            if (dialog.ShowDialog() == true)
            {
                localFilePath = dialog.FileName.ToString(); //獲得檔案路徑
                fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);  //獲取檔名，不帶路徑
            }
            else
            {
                return;
            }

            Excel.Application xlApp = new Excel.Application();
            Excel.Workbook workBook = null;
            Excel.Worksheet sample_sheet = null;
            Excel.Worksheet sheet = null;
            Excel.Range insert_row = null;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                DateTime QryDtBeg = Qry_Beg_Dt.SelectedDate == null ? DateTime.Now : (DateTime)Qry_Beg_Dt.SelectedDate;
                DateTime QryDtEnd = Qry_End_Dt.SelectedDate == null ? DateTime.Now : (DateTime)Qry_End_Dt.SelectedDate;
                QryDtBeg = new DateTime(QryDtBeg.Year, QryDtBeg.Month, 1);  //起始日：月初
                QryDtEnd = new DateTime(QryDtEnd.Year, QryDtEnd.Month, DateTime.DaysInMonth(QryDtEnd.Year, QryDtEnd.Month));  //結束日：月底日

                List<BookBase> Books = GetSelectedBook();  //取得帳冊清單(含總帳)
                DataTable InvMast = GetInvMast(QryDtBeg, QryDtEnd, Books);  //取得交易
                int DataCount = InvMast.AsEnumerable().Count();  //行數

                #region 開始處理Excel
                xlApp.DisplayAlerts = false;
                xlApp.Visible = false;
                xlApp.ScreenUpdating = false;

                workBook = xlApp.Workbooks.Open(System.Environment.CurrentDirectory + @"\Reports\Report_R002.xlsx");
                sample_sheet = workBook.Worksheets[1];

                string TW_Year_Beg = (QryDtBeg.Year - 1911).ToString() + $"/{QryDtBeg.ToString("%M/%d")}";  //民國年月日
                string TW_Year_End = (QryDtEnd.Year - 1911).ToString() + $"/{QryDtEnd.ToString("%M/%d")}";  //民國年月日
                int skiprow = 5;  //表頭(含餘額)
                int cur_row = skiprow;

                //複製sheet
                sample_sheet.Copy(Type.Missing, workBook.Sheets[workBook.Sheets.Count]); // copy                
                workBook.Sheets[workBook.Sheets.Count].Name = $@"{AppVar.User.dept_name}交易明細表";  // rename

                sheet = workBook.Sheets[workBook.Sheets.Count];
                sheet.Cells[2, 1] = AppVar.User.dept_name;
                sheet.Cells[3, 1] = $@"{TW_Year_Beg}~{TW_Year_End} 庫存明細表";

                foreach (var r in InvMast.AsEnumerable())
                {
                    cur_row += 1;

                    sheet.Cells[cur_row, 1] = r.Field<string>("acct_book");
                    sheet.Cells[cur_row, 2] = r.Field<string>("book_name");
                    sheet.Cells[cur_row, 3] = r.Field<string>("book_type_desc");
                    sheet.Cells[cur_row, 4] = r.Field<DateTime>("trade_dt");
                    sheet.Cells[cur_row, 5] = r.Field<decimal>("amt");

                    //標註總帳冊
                    if (r.Field<string>("acct_book") == "Total")
                    {
                        sheet.Range[$"A{cur_row}:E{cur_row}"].Cells.Font.Color = Excel.XlRgbColor.rgbDarkBlue;
                    }
                }

                workBook.SaveAs(localFilePath);
                new MessageBoxCustom($"{fileNameExt}報表列印成功", "成功", MessageButtons.Ok, MessageType.Success).ShowDialog();
                #endregion

            }
            catch (Exception ex)
            {
                new MessageBoxCustom("列印失敗，錯誤訊息：" + ex.Message.ToString(), "", MessageButtons.Ok, MessageType.Error).ShowDialog();
            }
            finally
            {
                if (insert_row != null) System.Runtime.InteropServices.Marshal.FinalReleaseComObject(insert_row);
                if (sheet != null) System.Runtime.InteropServices.Marshal.FinalReleaseComObject(sheet);
                if (sample_sheet != null) System.Runtime.InteropServices.Marshal.FinalReleaseComObject(sample_sheet);
                if (workBook != null)
                {
                    workBook.Close(false);
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(workBook);
                }
                if (xlApp != null)
                {
                    xlApp.Workbooks.Close();
                    xlApp.Quit();
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(xlApp);
                }
                //下面是強制關閉Excel語法，逼不得已才使用
                System.Diagnostics.Process[] excelProcess = System.Diagnostics.Process.GetProcessesByName("EXCEL");
                foreach (var item in excelProcess)
                {
                    if (item.MainWindowTitle == "")  //利用是否有視窗來判斷為背景程式
                        item.Kill();
                }

                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private DataTable GetInvMast(DateTime QryDtBeg, DateTime QryDtEnd, List<BookBase> Books)
        {
            string q_beg_dt = QryDtBeg.GetFullDate();
            string q_end_dt = QryDtEnd.GetFullDate();
            string q_books = "";
            foreach (var b in Books)
            {
                q_books += $@"'{b.book}',";
            }
            q_books = q_books.TrimEnd(',');

            DataTable dtb = DBService.SQL_QryTable($@"
select
	A.*,
	B.book_name,
	C.item_name as book_type_desc
from inv_mast A
left join book_base B on A.acct_book = B.book
left join map_file C on B.book_type = C.item and C.opt_no = 'book_type'
where 1=1
	and date(trade_dt) between '{q_beg_dt}' and '{q_end_dt}'
	and A.acct_book in ({q_books})
order by trade_dt, acct_book
", new string[] { "trade_dt", "logtime" });
            return dtb;
        }
    }
}
