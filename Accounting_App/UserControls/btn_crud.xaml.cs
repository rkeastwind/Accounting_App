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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Accounting_App.Utilities;
using Accounting_App.DTO;

namespace Accounting_App.UserControls
{
    /// <summary>
    /// btn_crud.xaml 的互動邏輯
    /// </summary>
    public partial class btn_crud : UserControl
    {
        public BasRolePermission permission { get; set; }

        public btn_crud()
        {
            InitializeComponent();
        }

        public void Refresh(FormStateS st)
        {
            //控制Visibility
            if (st == FormStateS.Initial || st == FormStateS.ShowData)
            {
                BtnGroup_CED.Visibility = System.Windows.Visibility.Visible;
                BtnGroup_SoN.Visibility = System.Windows.Visibility.Hidden;
                SetByPermission();
            }
            else
            {
                BtnGroup_CED.Visibility = System.Windows.Visibility.Hidden;
                BtnGroup_SoN.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public bool CanEditDelete
        {
            set
            {
                Btn_Edit.IsEnabled = Btn_Delete.IsEnabled = value;
                SetByPermission();
            }
        }

        /// <summary>
        /// 權限控制
        /// </summary>
        private void SetByPermission()
        {
            //因為預設可用，若設定為false就強制禁用
            if (permission != null)
            {
                if (permission.cmd_Add == false) Btn_Add.IsEnabled = false;
                if (permission.cmd_Edit == false) Btn_Edit.IsEnabled = false;
                if (permission.cmd_Delete == false) Btn_Delete.IsEnabled = false;
            }
        }
    }
}
