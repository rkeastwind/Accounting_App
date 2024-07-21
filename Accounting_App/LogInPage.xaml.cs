using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Accounting_App
{
    /// <summary>
    /// LogInPage.xaml 的互動邏輯
    /// </summary>
    public partial class LogInPage : Window
    {
        public LogInPage()
        {
            InitializeComponent();
            SetRememberMe(0);
        }

        private void Btn_LogIn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                lb_Message.Content = "";
                if (string.IsNullOrWhiteSpace(Txt_UserName.Text.Trim()))
                {
                    lb_Message.Content = $"請輸入帳號";
                    return;
                }
                if (string.IsNullOrEmpty(Txt_Password.Password.Trim()))
                {
                    lb_Message.Content = $"請輸入密碼";
                    return;
                }
                var user = DBService.GetBasUser(Txt_UserName.Text, Txt_Password.Password);
                if (user == null)
                {
                    lb_Message.Content = $"查無帳號或密碼錯誤";
                    return;
                }
                AppVar.User = user;
                if (user.enabled == false)
                {
                    lb_Message.Content = $"帳號已被停用";
                    return;
                }

                SetRememberMe(1);

                MainWindow w = new MainWindow();
                w.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void SetRememberMe(int mod)
        {
            if (mod == 0)  //讀取
            {
                string LogInDefaultUser = ConfigurationManager.AppSettings["LogInDefaultUser"];
                if (LogInDefaultUser != null)
                {
                    Txt_UserName.Text = LogInDefaultUser;
                    chk_RememberMe.IsChecked = true;
                }
            }
            else  //寫入，一律先刪除，有打勾才更新值
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove("LogInDefaultUser");
                if (chk_RememberMe.IsChecked == true)
                    config.AppSettings.Settings.Add("LogInDefaultUser", Txt_UserName.Text);
                config.Save(ConfigurationSaveMode.Minimal);
            }
        }
    }
}
