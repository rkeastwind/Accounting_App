using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Accounting_App.UserControls
{
    /// <summary>
    /// Interaction logic for MessageBoxCustom.xaml
    /// </summary>
    public partial class MessageBoxCustom : Window
    {
        public MessageBoxCustom(string Message, MessageButtons Buttons, MessageType Type)
        {
            FormLoad(Message, null, Buttons, Type);
        }
        public MessageBoxCustom(string Message, string Title, MessageButtons Buttons, MessageType Type)
        {
            FormLoad(Message, Title, Buttons, Type);
        }

        public void FormLoad(string Message, string Title, MessageButtons Buttons, MessageType Type)
        {
            InitializeComponent();
            var ph = new PaletteHelper();
            var theme = ph.GetTheme();

            txtMessage.Text = Message;
            switch (Type)
            {
                case MessageType.Info:
                    changeBackgroundThemeColor(theme.PrimaryMid.Color);
                    txtTitle.Text = Title ?? "Info";
                    break;
                case MessageType.Confirmation:
                    changeBackgroundThemeColor(theme.SecondaryMid.Color);
                    txtTitle.Text = Title ?? "Confirmation";
                    break;
                case MessageType.Warning:
                    {
                        changeBackgroundThemeColor(theme.SecondaryMid.Color);
                        txtTitle.Text = Title ?? "Warning";
                    }
                    break;
                case MessageType.Success:
                    {
                        string defaultColor = "#336633";
                        Color bkColor = (Color)ColorConverter.ConvertFromString(defaultColor);
                        changeBackgroundThemeColor(bkColor);
                        txtTitle.Text = Title ?? "Success";
                    }
                    break;
                case MessageType.Error:
                    {
                        string defaultColor = "#ED5565";
                        Color bkColor = (Color)ColorConverter.ConvertFromString(defaultColor);
                        changeBackgroundThemeColor(bkColor);
                        txtTitle.Text = Title ?? "Error";
                    }
                    break;
            }

            switch (Buttons)
            {
                case MessageButtons.OkCancel:
                    btnYes.Visibility = Visibility.Collapsed; btnNo.Visibility = Visibility.Collapsed;
                    break;
                case MessageButtons.YesNo:
                    btnOk.Visibility = Visibility.Collapsed; btnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageButtons.Ok:
                    btnOk.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Collapsed;
                    btnYes.Visibility = Visibility.Collapsed; btnNo.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        public void changeBackgroundThemeColor(Color newColor)
        {
            cardHeader.Background = new SolidColorBrush(newColor);
            btnClose.Foreground = new SolidColorBrush(newColor);
            btnYes.Background = btnYes.BorderBrush = new SolidColorBrush(newColor);
            btnNo.Background = btnNo.BorderBrush = new SolidColorBrush(newColor);

            btnOk.Background = btnOk.BorderBrush = new SolidColorBrush(newColor);
            btnCancel.Background = btnCancel.BorderBrush = new SolidColorBrush(newColor);
        }
        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
    public enum MessageType
    {
        Info,
        Confirmation,
        Success,
        Warning,
        Error,
    }
    public enum MessageButtons
    {
        OkCancel,
        YesNo,
        Ok,
    }
}
