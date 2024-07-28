using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf.Themes.Internal;

namespace Accounting_App.UserControls
{
    public class CurrencyTextBox : TextBox
    {
        public CurrencyTextBox()
        {
            this.Text = "0";
            this.TextAlignment = TextAlignment.Right;

            this.PreviewTextInput += defaultPreviewTextInput;
            DataObject.AddPastingHandler(this, defaultTextBoxPasting);
            this.GotFocus += Txt_GotFocus;
            this.LostFocus += Txt_LostFocus;
        }

        private bool IsTextAllowed(TextBox textBox, String text)
        {
            Regex regex = new Regex("^[1-9][0-9]*$");  //正整數
            String newText = textBox.Text.Insert(textBox.CaretIndex, text);
            return regex.IsMatch(newText);
        }

        private void defaultTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));

                if (!IsTextAllowed((TextBox)sender, text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void defaultPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (IsTextAllowed((TextBox)sender, e.Text))
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        //關注：要還原字串並全選
        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            var obj = (TextBox)sender;
            Text = Value.ToString();
            this.SelectAll();
        }

        //取消關注：加上千分位
        private void Txt_LostFocus(object sender, RoutedEventArgs e)
        {
            var obj = (TextBox)sender;
            if (string.IsNullOrEmpty(obj.Text.Trim()))
                Text = "0";
            else
            {
                Text = Convert.ToDecimal(Text).ToString("N0");  //千分位符號
            }
        }

        public decimal Value
        {
            get
            {
                if (string.IsNullOrEmpty(Text.Trim()))
                    return 0;
                else
                    return Convert.ToDecimal(Text.Replace(",", ""));
            }
            set
            {
                Text = value.ToString("N0");  //千分位符號
            }
        }
    }
}
