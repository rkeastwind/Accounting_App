using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Accounting_App.DTO;
using Accounting_App.Utilities;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;
using Microsoft.Win32;
using System.IO;

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_sql_repair_tool.xaml 的互動邏輯
    /// </summary>
    public partial class Form_sql_repair_tool : Window
    {
        public Form_sql_repair_tool()
        {
            InitializeComponent();
            FormInitial();
        }

        private void FormInitial()
        {
            DataTable d = DBService.SQL_QryTable("SELECT name AS 【TableList】 FROM sqlite_master WHERE type='table' ORDER BY name", new string[] { });
            Txt_TableList.Text = CommUtility.SaveCSV(d);
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //前6碼select為查詢，否則視為執行
                if (Txt_Input.Text.Substring(0, 6).ToLower() == "select")
                {
                    DataTable d = DBService.SQL_QryTable(Txt_Input.Text, new string[] { });
                    Txt_Output.Text = CommUtility.SaveCSV(d);
                    Inline st = new Run($"查詢成功，共 {d.Rows.Count} 筆資料" + "\r\n") { Foreground = new SolidColorBrush(Colors.RoyalBlue) };
                    Paragraph p = (Paragraph)Ritx_Stas.Document.Blocks.FirstOrDefault();
                    p.Inlines.InsertBefore(p.Inlines.FirstInline, st);
                }
                else
                {
                    int r = DBService.SQL_Command(Txt_Input.Text);
                    Inline st = new Run($@"執行成功，異動 {r} 筆資料" + "\r\n") { Foreground = new SolidColorBrush(Colors.Crimson) };
                    Paragraph p = (Paragraph)Ritx_Stas.Document.Blocks.FirstOrDefault();
                    //p.Inlines.Add(st);
                    if (p.Inlines.FirstInline == null)
                        p.Inlines.Add(st);
                    else
                        p.Inlines.InsertBefore(p.Inlines.FirstInline, st);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("執行失敗，錯誤訊息：" + ex.Message.ToString());
            }
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            string localFilePath = "";
            string fileNameExt = "";

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.RestoreDirectory = true;  //儲存對話方塊是否記憶上次開啟的目錄 
            dialog.Title = "請選擇資料夾";
            dialog.Filter = "CSV(*.csv)|*.csv";
            if (dialog.ShowDialog() == true)
            {
                localFilePath = dialog.FileName.ToString(); //獲得檔案路徑
                fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);  //獲取檔名，不帶路徑
            }
            else
            {
                return;
            }
            try
            {
                FileStream fs = new FileStream(localFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
                sw.WriteLine(Txt_Output.Text);
                sw.Close();
                fs.Close();
                MessageBox.Show($"{fileNameExt}儲存成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("儲存失敗，錯誤訊息：" + ex.Message.ToString());
            }
        }
    }
}
