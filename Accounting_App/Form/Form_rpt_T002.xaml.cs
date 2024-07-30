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

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_rpt_T002.xaml 的互動邏輯
    /// </summary>
    public partial class Form_rpt_T002 : Window
    {
        public Form_rpt_T002()
        {
            InitializeComponent();
            BindBookBase();
        }

        private void BindBookBase()
        {
            DataTable book = DBService.SQL_QryTable(@"
select A.*, B.item_name as book_type_desc
from book_base A
left join map_file B on A.book_type = B.item and B.opt_no = 'book_type'
where book_type != 0
order by book", new string[] { });
            DG_Main.ItemsSource = book.AsDataView();
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

        private List<string> GetSelectedBook()
        {
            List<string> book = new List<string>();
            foreach (DataRowView r in DG_Main.SelectedItems)
            {
                book.Add(r.Row[0].ToString());
            }
            return book;
        }


        private bool CheckSelectDate()
        {
            if (DG_Main.SelectedItems.Count == 0)
            {
                MessageBox.Show("請選擇帳冊", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (Qry_Beg_Dt.SelectedDate == null || Qry_End_Dt.SelectedDate == null)
            {
                MessageBox.Show("查詢日期不可空白", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            DateTime QryDtBeg = (DateTime)Qry_Beg_Dt.SelectedDate;
            DateTime QryDtEnd = (DateTime)Qry_End_Dt.SelectedDate;
            if (QryDtBeg.Year > QryDtEnd.Year || (QryDtBeg.Year == QryDtEnd.Year && QryDtBeg.Month > QryDtEnd.Month))  //年份大於，或年份相同但月份大於
            {
                MessageBox.Show("起日不可大於迄日", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                List<BookBase> Books = DBService.GetBookBase(false).Where(x => GetSelectedBook().Contains(x.book)).ToList();  //取得帳冊清單
                DataTable Trans = GetTrans(QryDtBeg, QryDtEnd);  //取得交易

                #region 開始處理Excel
                xlApp.DisplayAlerts = false;
                xlApp.Visible = false;
                xlApp.ScreenUpdating = false;

                workBook = xlApp.Workbooks.Open(System.Environment.CurrentDirectory + @"\Reports\Report_T002.xlsx");
                sample_sheet = workBook.Worksheets[1];

                DateTime cur_dt = QryDtBeg;
                while (cur_dt < QryDtEnd)  //日期
                {
                    foreach (var b in Books)  //帳冊
                    {
                        string cur_beg_dt = new DateTime(cur_dt.Year, cur_dt.Month, 1).GetFullDate();  //月初
                        string cur_end_dt = new DateTime(cur_dt.Year, cur_dt.Month, DateTime.DaysInMonth(cur_dt.Year, cur_dt.Month)).GetFullDate();  //月底日                        

                        DataRow[] cur_trans = Trans.Select($@"(trade_dt >= '{cur_beg_dt}' and trade_dt <= '{cur_end_dt}') and (acct_book_in = '{b.book}' or acct_book_out = '{b.book}')");  //當月帳冊交易
                        int DataCount = cur_trans.Count();  //行數
                        decimal LastInv = GetLastDateEndInv(cur_dt, b.book);  //上個月底餘額

                        string TW_Year = (cur_dt.Year - 1911).ToString();  //民國年
                        int skiprow = 8;  //表頭(含餘額)
                        int cur_row = skiprow;

                        //複製sheet
                        sample_sheet.Copy(Type.Missing, workBook.Sheets[workBook.Sheets.Count]); // copy                
                        workBook.Sheets[workBook.Sheets.Count].Name = $"{TW_Year}年度{cur_dt.Month}月{b.book_name}";  // rename

                        sheet = workBook.Sheets[workBook.Sheets.Count];
                        sheet.Cells[2, 1] = $"{TW_Year}年度 {cur_dt.Month}月";
                        sheet.Cells[3, 1] = $"{b.book_name}變動表";
                        sheet.Cells[4, 3] = AppVar.User.dept_name;
                        sheet.Cells[5, 3] = b.book_type == "2" ? $@"{b.bank_name}/{b.account}" : "";
                        sheet.Cells[8, 6] = LastInv;

                        //一次Insert行數(會複製10之上的那一行)
                        if (DataCount >= 2)
                        {
                            insert_row = sheet.Rows[10 + ":" + (10 + DataCount - 2)];
                            insert_row.Insert(Excel.XlInsertShiftDirection.xlShiftDown);
                        }

                        foreach (var r in cur_trans)
                        {
                            cur_row += 1;

                            sheet.Cells[cur_row, 1] = r.Field<DateTime>("trade_dt");
                            sheet.Cells[cur_row, 2] = r.Field<string>("acct_code_desc");
                            sheet.Cells[cur_row, 3] = r.Field<string>("memo");

                            if (r.Field<string>("acct_book_in") == b.book)
                            {
                                sheet.Cells[cur_row, 4] = r.Field<decimal>("amt");
                                sheet.Cells[cur_row, 5] = 0;
                            }
                            else
                            {
                                sheet.Cells[cur_row, 4] = 0;
                                sheet.Cells[cur_row, 5] = r.Field<decimal>("amt");
                            }

                            sheet.Cells[cur_row, 6].Formula = $@"=F{cur_row - 1}+D{cur_row}-E{cur_row}";
                        }
                        //刪除行
                        sheet.Rows[skiprow + DataCount + 1].delete();
                    }
                    cur_dt = cur_dt.AddMonths(1);
                }

                workBook.SaveAs(localFilePath);
                MessageBox.Show($"{fileNameExt}報表列印成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show("列印失敗，錯誤訊息：" + ex.Message.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
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

        private DataTable GetTrans(DateTime QryDtBeg, DateTime QryDtEnd)
        {
            string q_beg_dt = QryDtBeg.GetFullDate();
            string q_end_dt = QryDtEnd.GetFullDate();

            DataTable dtb = DBService.SQL_QryTable($@"
select
    A.*,
    case when A.action in ('1','2') then ifnull(B.item_name,'') else ifnull(C.item_name,'') end as acct_code_desc
from tra_mast A
left join map_file B on A.acct_code = B.item and B.opt_no = 'AC'
left join map_file C on A.action = C.item and C.opt_no = '1'
where 1=1
    and date(trade_dt) between '{q_beg_dt}' and '{q_end_dt}'
order by trade_dt, action, action_dtl
", new string[] { "trade_dt", "logtime" });
            return dtb;
        }

        private decimal GetLastDateEndInv(DateTime QryDt, string book)
        {
            decimal amt = 0;
            string q_end_dt = new DateTime(QryDt.Year, QryDt.Month, 1).AddDays(-1).GetFullDate();  //上個月底
            var dtb = DBService.QryInvMast($@"where date(trade_dt) = '{q_end_dt}' and acct_book = '{book}'");
            if (dtb.Count != 0)
                amt = dtb.FirstOrDefault().amt;

            return amt;
        }
    }
}
