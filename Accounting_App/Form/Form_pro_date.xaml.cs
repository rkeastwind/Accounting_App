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

        DataTable DT_Main = DBService.QryProDate("where 1=0");  //初始化

        public Form_pro_date()
        {
            InitializeComponent();
        }

        public Form_pro_date(string tl)
        {
            InitializeComponent();
            Title = tl;
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
            DT_Main = DBService.QryProDate(filter);
            DG_Main.ItemsSource = DT_Main.DefaultView;
        }

        private void Btn_Execute_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt = (DateTime)DT_Main.AsEnumerable().FirstOrDefault()["pro_dt"];

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (FormState == FormStates.Run)
                {
                    if (ProUtility.ExecutePro(dt, 1))
                    {
                        FormStateChange(FormStates.Run);
                        MessageBox.Show($"{dt.ToString("yyyy-MM-dd")}結帳成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    if (ProUtility.ExecutePro(dt, 0))
                    {
                        FormStateChange(FormStates.Cancel);
                        MessageBox.Show($"{dt.ToString("yyyy-MM-dd")}反結帳成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("結帳失敗，錯誤訊息：" + ex.Message.ToString());
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }

    /// <summary>
    /// Gride文字轉換器(顯示用，只實作Convert)
    /// </summary>
    public class ProDateGrideConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataRowView r = value as DataRowView;
            string col = parameter as string;
            string result = "";

            if (col == "pro_status")
            {
                if (r.Row[col].ToString() == "0")
                    result = "未結帳";
                else if (r.Row[col].ToString() == "1")
                    result = "已結帳";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataRowView r = value as DataRowView;
            string col = parameter as string;
            return r.Row[col].ToString();
        }
    }
}
