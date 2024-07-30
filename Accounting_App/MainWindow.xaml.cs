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
using System.Reflection;
using MaterialDesignThemes.Wpf;

namespace Accounting_App
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DBExists();
            InitializeComponent();
            SetFormInfo();
            BindMenu();
        }

        private void DBExists()
        {
            string DB_path = ConfigurationManager.AppSettings["DBPath"];
            if (!File.Exists(DB_path))
            {
                MessageBox.Show($"資料庫{Environment.CurrentDirectory}{DB_path.Substring(1)}不存在", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
            }
        }

        private void SetFormInfo()
        {
            tb_version.Text = ConfigurationManager.AppSettings["Version"];
            string UserInfo = $@"使用者：{AppVar.User.user_id}  {AppVar.User.name}
單位：{AppVar.User.dept_name}
角色權限：{AppVar.User.role_name}";
            tb_UserInfo.Text = UserInfo;
        }

        private void BindMenu()
        {
            try
            {
                var LsMenu = DBService.GetBasMenu(AppVar.User.role_id);
                var lsMenu = GetMenuList(LsMenu, "0");
                //從Root開始遞迴
                foreach (var m in lsMenu)
                {
                    var rtmu = SetMenuItems(m.child);
                    SetAMenuItem(ref rtmu, m);
                    MainMenu.Items.Add(rtmu);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 取得Menu清單樹
        /// </summary>
        public List<BasMenu> GetMenuList(List<BasMenu> lsmu, string ptkey)
        {
            List<BasMenu> result = new List<BasMenu>();
            foreach (var m in lsmu)
            {
                if (m.parent_id == ptkey)
                {
                    BasMenu tmu = new BasMenu();
                    tmu = m;
                    List<BasMenu> child = GetMenuList(lsmu, m.menu_id);
                    if (child != null && child.Count > 0)
                        tmu.child = child;
                    result.Add(tmu);
                }
            }
            return result;
        }

        /// <summary>
        /// 建立Item清單，從第二層開始
        /// </summary>
        public MenuItem SetMenuItems(List<BasMenu> lsmu)
        {
            MenuItem result = new MenuItem();
            foreach (var m in lsmu)
            {
                MenuItem mi = new MenuItem();
                if (m.child != null && m.child.Count > 0)  //有子層要遞迴
                    mi = SetMenuItems(m.child);

                SetAMenuItem(ref mi, m);
                result.Items.Add(mi);
            }
            return result;
        }

        /// <summary>
        /// 設定一個Item項目
        /// </summary>
        public void SetAMenuItem(ref MenuItem mi, BasMenu mu)
        {
            mi.Header = mu.menu_title;
            mi.Tag = mu;

            int icon_number = 0;
            if (int.TryParse(mu.icon_path, out icon_number))
            {
                PackIcon x = new PackIcon();
                x.Kind = (PackIconKind)icon_number;
                mi.Icon = x;
            }
            if (mu.menu_type == "1")
                mi.Click += MenuItem_Click;  //賦予事件
        }

        public void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            BasMenu mu = (BasMenu)mi.Tag;
            if (!IsWindowOpen<Window>(mu.menu_id))
            {
                AppVar.OpenMenuId = mu.menu_id;  //因為不想多一個建構式，用全域參數實作
                Window frm = (Window)System.Reflection.Assembly.Load("Accounting_App").CreateInstance(mu.page_url, false, BindingFlags.Instance | BindingFlags.Public, null, null, null, null);
                if (frm == null)
                    throw new Exception("查無畫面路徑" + mu.page_url);
                frm.Title = mu.menu_title;
                frm.Tag = mu.menu_id;
                frm.Show();
            }
        }

        //判斷視窗是否開啟(泛型)，若存在就active
        public static bool IsWindowOpen<T>(string MenuId = "") where T : Window
        {
            bool b = string.IsNullOrEmpty(MenuId)
               ? Application.Current.Windows.OfType<T>().Any()
               : Application.Current.Windows.OfType<T>().Any(w => Convert.ToString(w.Tag).Equals(MenuId));
            if (b == true)
            {
                if (string.IsNullOrEmpty(MenuId))
                    Application.Current.Windows.OfType<T>().FirstOrDefault().Activate();
                else
                    Application.Current.Windows.OfType<T>().Where(w => Convert.ToString(w.Tag).Equals(MenuId)).FirstOrDefault().Activate();
            }
            return b;
        }

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
