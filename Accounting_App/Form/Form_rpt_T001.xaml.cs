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
using System.Windows.Documents;

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_rpt_T001.xaml 的互動邏輯
    /// </summary>
    public partial class Form_rpt_T001 : System.Windows.Window
    {
        public Form_rpt_T001()
        {
            InitializeComponent();
        }

        private bool CheckSelectDate()
        {
            if (Qry_Year.SelectedDate == null)
            {
                MessageBox.Show("查詢日期不可空白", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Error);
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
                DateTime QryDt = Qry_Year.SelectedDate == null ? DateTime.Now : (DateTime)Qry_Year.SelectedDate;

                decimal LastInv = GetLastYesrEndInv(QryDt);  //去年底庫存
                DataTable MainTable = GetTable(QryDt);  //取得基底
                if (MainTable.Rows.Count == 0)
                {
                    MessageBox.Show("查無資料", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                int DataCount = MainTable.AsEnumerable().Count();  //行數

                #region 開始處理Excel                
                xlApp.DisplayAlerts = false;
                xlApp.Visible = false;
                xlApp.ScreenUpdating = false;

                workBook = xlApp.Workbooks.Open(System.Environment.CurrentDirectory + @"\Reports\Report_T001.xlsx");
                sample_sheet = workBook.Worksheets[1];

                string TW_Year = (QryDt.Year - 1911).ToString();  //民國年
                int skiprow = 7;  //表頭(含餘額)
                int cur_row = skiprow;
                int cur_month = 0;

                //複製sheet
                sample_sheet.Copy(Type.Missing, workBook.Sheets[workBook.Sheets.Count]); // copy                
                workBook.Sheets[workBook.Sheets.Count].Name = $@"{AppVar.User.dept_name}{TW_Year}年度經常費收支表";  // rename

                sheet = workBook.Sheets[workBook.Sheets.Count];
                sheet.Cells[2, 1] = AppVar.User.dept_name;
                sheet.Cells[3, 1] = $@"{TW_Year}年度經常費收支表";
                sheet.Cells[5, 1] = TW_Year;
                sheet.Cells[7, 7] = LastInv;

                //一次Insert行數(會複製9之上的那一行)
                if (DataCount >= 2)
                {
                    insert_row = sheet.Rows[9 + ":" + (9 + DataCount - 2)];
                    insert_row.Insert(Excel.XlInsertShiftDirection.xlShiftDown);
                }

                foreach (var r in MainTable.AsEnumerable())
                {
                    cur_row += 1;

                    //畫線
                    if (cur_month != 0 && cur_month != r.Field<DateTime>("trade_dt").Month)
                    {
                        sheet.Range[$"A{cur_row}:G{cur_row}"].Cells.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                        sheet.Range[$"A{cur_row}:G{cur_row}"].Cells.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThin;
                    }
                    cur_month = r.Field<DateTime>("trade_dt").Month;

                    sheet.Cells[cur_row, 1] = r.Field<DateTime>("trade_dt").Month;
                    sheet.Cells[cur_row, 2] = r.Field<DateTime>("trade_dt").Day;
                    sheet.Cells[cur_row, 3] = r.Field<string>("acct_code_desc");
                    sheet.Cells[cur_row, 4] = r.Field<string>("memo");

                    if (r.Field<string>("action") == "1")
                    {
                        sheet.Cells[cur_row, 5] = r.Field<decimal>("amt");
                        sheet.Cells[cur_row, 6] = 0;
                    }
                    else
                    {
                        sheet.Cells[cur_row, 5] = 0;
                        sheet.Cells[cur_row, 6] = r.Field<decimal>("amt");
                    }

                    sheet.Cells[cur_row, 7].Formula = $@"=G{cur_row - 1}+E{cur_row}-F{cur_row}";
                }
                //刪除行
                sheet.Rows[skiprow + DataCount + 1].delete();

                //畫線
                sheet.Range[$"A{skiprow + DataCount + 1}:G{skiprow + DataCount + 1}"].Cells.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = Excel.XlLineStyle.xlContinuous;
                sheet.Range[$"A{skiprow + DataCount + 1}:G{skiprow + DataCount + 1}"].Cells.Borders[Excel.XlBordersIndex.xlEdgeTop].Weight = Excel.XlBorderWeight.xlThin;

                workBook.SaveAs(localFilePath);
                MessageBox.Show($"{fileNameExt}報表列印成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                #endregion

            }
            catch (Exception ex)
            {
                MessageBox.Show("列印失敗，錯誤訊息：" + ex.Message.ToString());
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

        private DataTable GetTable(DateTime QryDt)
        {
            string q_beg_dt = QryDt.ToString("yyyy") + "-01-01";  //年初
            string q_end_dt = QryDt.ToString("yyyy") + "-12-31";  //年底

            DataTable dtb = DBService.SQL_QryTable($@"
select
    A.*,
    ifnull(B.item_name,'') as acct_code_desc
from tra_mast A
left join map_file B on A.acct_code = B.item and B.opt_no = 'AC'
where 1=1
    and action in ('1','2')
    and date(trade_dt) between '{q_beg_dt}' and '{q_end_dt}'
order by trade_dt, action, action_dtl
", new string[] { "trade_dt", "logtime" });
            return dtb;
        }

        private decimal GetLastYesrEndInv(DateTime QryDt)
        {
            decimal amt = 0;
            string q_end_dt = (QryDt.Year - 1).ToString() + "-12-31";  //去年底
            var dtb = DBService.QryInvMast($@"where date(trade_dt) = '{q_end_dt}' and acct_book = 'Total'");
            if (dtb.Count != 0)
                amt = dtb.FirstOrDefault().amt;

            return amt;
        }
    }
}
