using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
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

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_chg_pwd.xaml 的互動邏輯
    /// </summary>
    public partial class Form_chg_pwd : Window
    {
        public Form_chg_pwd()
        {
            InitializeComponent();
        }

        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBefoeSave()) return;  //檢核
            AppVar.User.password = Txt_NewPwd.Password;
            AppVar.User.loguser = AppVar.User.user_id;
            AppVar.User.logtime = DateTime.Now;
            AppVar.User.UpdateDB();
            MessageBox.Show("密碼更新成功");
            Close();
        }

        private bool CheckBefoeSave()
        {
            if (AppVar.User.password != Txt_OrgPwd.Password)
            {
                lb_Message.Content = "原密碼有誤";
                return false;
            }
            if (Txt_NewPwd.Password != Txt_NewPwd2.Password)
            {
                lb_Message.Content = "兩次輸入的新密碼不同";
                return false;
            }
            return true;
        }
    }
}
