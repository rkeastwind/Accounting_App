using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Accounting_App.UserControls
{
    public class NumberTextBox : TextBox
    {
        public NumberTextBox()
        {
            this.PreviewTextInput += defaultPreviewTextInput;
            DataObject.AddPastingHandler(this, defaultTextBoxPasting);
        }

        private bool IsTextAllowed(TextBox textBox, String text)
        {
            Regex regex = new Regex("^[0-9]*$");
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
    }
}
