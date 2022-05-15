using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Accounting_App.DTO;
using Accounting_App.Utilities;
using Accounting_App.Form;
using System.Globalization;
using System.Configuration;
using System.IO;

namespace Accounting_App
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        UserInfo userInfo = new UserInfo();

        public MainWindow()
        {
            DBExists();
            InitializeComponent();
            RefreshUserInfo();
        }

        private void DBExists()
        {
            string DB_path = ConfigurationManager.AppSettings["DBPath"];
            if (!File.Exists(DB_path))
            {
                MessageBox.Show($"資料庫{Environment.CurrentDirectory}{DB_path.Substring(1)}不存在", "", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }

        //重整使用者資訊
        private void RefreshUserInfo()
        {
            AppVar.RefreshUser();
            Txt_UserName.Text = AppVar.UserName;
            Txt_Department.Text = AppVar.Department;
        }

        //更新使用者資訊
        private void Btn_UpdUseInfo_Click(object sender, RoutedEventArgs e)
        {
            DBService.UpdUserInfo(new UserInfo { UserName = Txt_UserName.Text, Department = Txt_Department.Text });
            RefreshUserInfo();
            MessageBox.Show("更新使用者資訊成功", "", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }



        #region 開啟頁面      

        //判斷視窗是否開啟(泛型)，若存在就active
        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            bool b = string.IsNullOrEmpty(name)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));

            if (b == true)
            {
                if (string.IsNullOrEmpty(name))
                    Application.Current.Windows.OfType<T>().FirstOrDefault().Activate();
                else
                    Application.Current.Windows.OfType<T>().Where(w => w.Name.Equals(name)).FirstOrDefault().Activate();
            }
            return b;
        }

        private void MenuItem_Trade_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_tra_trade>())
            {
                MenuItem i = (MenuItem)sender;
                Form_tra_trade w = new Form_tra_trade(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_BankDeal_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_tra_bankdeal>())
            {
                MenuItem i = (MenuItem)sender;
                Form_tra_bankdeal w = new Form_tra_bankdeal(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_ProDate_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_pro_date>())
            {
                MenuItem i = (MenuItem)sender;
                Form_pro_date w = new Form_pro_date(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_Report_R001_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_rpt_R001>())
            {
                MenuItem i = (MenuItem)sender;
                Form_rpt_R001 w = new Form_rpt_R001(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_Report_R002_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_rpt_R002>())
            {
                MenuItem i = (MenuItem)sender;
                Form_rpt_R002 w = new Form_rpt_R002(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_Report_T001_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_rpt_T001>())
            {
                MenuItem i = (MenuItem)sender;
                Form_rpt_T001 w = new Form_rpt_T001(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_Report_T002_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_rpt_T002>())
            {
                MenuItem i = (MenuItem)sender;
                Form_rpt_T002 w = new Form_rpt_T002(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_BookBase_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_book_base>())
            {
                MenuItem i = (MenuItem)sender;
                Form_book_base w = new Form_book_base(i.Header.ToString());
                w.Show();
            }
        }

        private void MenuItem_SqlRepairTool_Click(object sender, RoutedEventArgs e)
        {
            if (!IsWindowOpen<Form_sql_repair_tool>())
            {
                MenuItem i = (MenuItem)sender;
                Form_sql_repair_tool w = new Form_sql_repair_tool(i.Header.ToString());
                w.Show();
            }
        }


        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("是否要關閉系統?", "確認", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                Environment.Exit(0);
            }
        }
    }
}
