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

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_pro_date.xaml 的互動邏輯
    /// </summary>
    public partial class Form_pro_date : Window
    {
        enum FormStates { Run, Cancel }
        enum FormStatesText { 結帳, 反結帳 }
        FormStates FormState = FormStates.Run;

        public Form_pro_date()
        {
            InitializeComponent();
        }

        private void Rb_run_Checked(object sender, RoutedEventArgs e)
        {
            FormStateChange(FormStates.Run);
        }

        private void Rb_cancel_Checked(object sender, RoutedEventArgs e)
        {
            FormStateChange(FormStates.Cancel);
        }

        private void FormStateChange(FormStates IformStates)
        {
            FormState = IformStates;
            if (Txt_Btn_Run != null)
            {
                Txt_Btn_Run.Text = Enum.GetName(typeof(FormStatesText), FormState);
                Btn_Execute.Background = FormState == FormStates.Run ? new SolidColorBrush(Color.FromArgb(255, 51, 102, 5)) : new SolidColorBrush(Color.FromArgb(255, 255, 127, 63));
            }

            string filter = FormState == FormStates.Run ?
                "where pro_dt = (select MIN(pro_dt) from pro_date where pro_status = 0)" :
                "where pro_dt = (select MAX(pro_dt) from pro_date where pro_status = 1)";
            var DT_Main = DBService.QryProDate(filter);
            DG_Main.ItemsSource = DT_Main;
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = (DG_Main.ItemsSource as List<ProDate>).FirstOrDefault().pro_dt;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (FormState == FormStates.Run)
                {
                    if (ProExec.ExecutePro(dt, 1))
                    {
                        FormStateChange(FormStates.Run);
                        MessageBox.Show($"{dt.GetFullDate()}結帳成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    if (ProExec.ExecutePro(dt, 0))
                    {
                        FormStateChange(FormStates.Cancel);
                        MessageBox.Show($"{dt.GetFullDate()}反結帳成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("結帳失敗，錯誤訊息：" + ex.Message.ToString(), "結帳失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
