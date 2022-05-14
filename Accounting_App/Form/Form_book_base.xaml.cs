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

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_book_base.xaml 的互動邏輯
    /// </summary>
    public partial class Form_book_base : Window
    {
        enum FormStates { Initial, ShowData, Add, Edit, Delete }
        enum FormStatesText { 初始, 顯示, 新增, 修改, 刪除 }
        FormStates FormState = FormStates.Initial;

        List<MapFile> Lst_BookType = DBService.GetMapFile("book_type");
        List<MapFile> Lst_BookType_NT = DBService.GetMapFile("book_type").Where(x => x.item != "0").ToList();

        DataTable DT_Main = DBService.QryBookBase();  //初始化


        public Form_book_base()
        {
            InitializeComponent();
        }

        public Form_book_base(string tl)
        {
            InitializeComponent();
            Title = tl;
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(FormStates.Initial);
        }

        //新增按鈕
        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeEdit(FormStates.Add)) return;
            Refresh(FormStates.Add);
        }

        //修改按鈕
        private void Btn_Edit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeEdit(FormStates.Edit)) return;
            Refresh(FormStates.Edit);
        }

        //刪除按鈕
        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBeforeEdit(FormStates.Delete)) return;
            Refresh(FormStates.Delete);
        }

        //儲存按鈕
        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBefoeSave()) return;  //檢核
            string s = Enum.GetName(typeof(FormStatesText), FormState);
            try
            {
                switch (FormState)
                {
                    case FormStates.Add:
                        Operation_Add();
                        break;
                    case FormStates.Edit:
                        Operation_Edit();
                        break;
                    case FormStates.Delete:
                        Operation_Delete();
                        break;
                }
                lbStatusBar2.Text = $"{s}成功";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{s}失敗，錯誤訊息：" + ex.Message.ToString());
            }
            finally
            {
                Refresh(FormStates.ShowData);
            }
        }

        //取消按鈕
        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Refresh(FormStates.ShowData);
        }

        //更新畫面與狀態
        private void Refresh(FormStates IformStates)
        {
            FormState = IformStates;
            BtnPanelControl();
            ObjectControl();
        }

        //CRUD按鈕控制
        private void BtnPanelControl()
        {
            //控制Visibility
            if (FormState == FormStates.Initial || FormState == FormStates.ShowData)
            {
                BtnGroup_CED.Visibility = System.Windows.Visibility.Visible;
                BtnGroup_SoN.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                BtnGroup_CED.Visibility = System.Windows.Visibility.Hidden;
                BtnGroup_SoN.Visibility = System.Windows.Visibility.Visible;
            }
        }

        //畫面物件控制
        private void ObjectControl()
        {
            //大物件控制
            if (FormState == FormStates.Initial || FormState == FormStates.ShowData)
            {
                GpBox_Dtl.IsHitTestVisible = false;
                DG_Main.IsHitTestVisible = true;
            }
            else if (FormState == FormStates.Add || FormState == FormStates.Edit)
            {
                GpBox_Dtl.IsHitTestVisible = true;
                DG_Main.IsHitTestVisible = false;
            }
            else if (FormState == FormStates.Delete)
            {
                GpBox_Dtl.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }

            //小物件控制
            if (FormState == FormStates.Add)  //先觸發改變選單，再觸發SelectionChanged
                Cmb_BookType.ItemsSource = Lst_BookType_NT;
            else
                Cmb_BookType.ItemsSource = Lst_BookType;

            if (FormState == FormStates.Initial || FormState == FormStates.ShowData)
            {
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState == FormStates.Add)
            {
                Cmb_BookType.SelectedIndex = 0;
                Txt_Book.Text = Txt_Book_Name.Text = Txt_Bank.Text = Txt_Bank_Name.Text = Txt_Account.Text = Txt_Title.Text = "";
            }
            Txt_Book.IsHitTestVisible = Cmb_BookType.IsHitTestVisible = !(FormState == FormStates.Edit);  //新增後不可編輯
        }

        //物件與Grid連動
        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Main.SelectedItems.Count > 0)
            {
                foreach (var r in DG_Main.SelectedItems)
                {
                    DataRowView drv = r as DataRowView;
                    Txt_Book.Text = drv["book"].ToString();
                    Txt_Book_Name.Text = drv["book_name"].ToString();
                    Cmb_BookType.SelectedValue = drv["book_type"];
                    Txt_Bank.Text = drv["bank"].ToString();
                    Txt_Bank_Name.Text = drv["bank_name"].ToString();
                    Txt_Account.Text = drv["account"].ToString();
                    Txt_Title.Text = drv["title"].ToString();
                }
            }

            //按鈕控制(選擇一列時才允許Edit和Delete)
            Btn_Edit.IsEnabled = Btn_Delete.IsEnabled = (DG_Main.SelectedItems.Count == 1);

            if (Txt_Book.Text == "Total")  //總帳冊不可修改
                Btn_Edit.IsEnabled = Btn_Delete.IsEnabled = false;
        }

        /// <summary>
        /// 修改前檢核
        /// </summary>
        /// <param name="ChangeState">要改變的State</param>
        /// <returns></returns>
        private bool CheckBeforeEdit(FormStates ChangeState)
        {
            if (ChangeState == FormStates.Delete)  //只卡刪除，修改仍可調整銀行資訊
            {
                if (!CommUtility.CheckBookIsPro(Txt_Book.Text))
                {
                    MessageBox.Show($"{Txt_Book.Text}帳冊已經結帳，" +
                        $"不可{Enum.GetName(typeof(FormStatesText), ChangeState)}", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState == FormStates.Add)
            {
                DataTable IsBookExists = DBService.QryBookBase($@"where book = '{Txt_Book.Text}'");
                if (IsBookExists.Rows.Count > 0)
                {
                    MessageBox.Show("帳冊代號已存在", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            if (FormState == FormStates.Add || FormState == FormStates.Edit)
            {
                if (Txt_Book.Text == "")
                {
                    MessageBox.Show("帳冊代號必填", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        //新增作業
        private void Operation_Add()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            DataRowView rowView = drv.AddNew();
            rowView["book"] = Txt_Book.Text.Trim();
            rowView["book_name"] = Txt_Book_Name.Text.Trim();
            rowView["book_type"] = Cmb_BookType.SelectedValue;
            rowView["bank"] = Txt_Bank.Value.ToString().Trim();
            rowView["bank_name"] = Txt_Bank_Name.Text.Trim();
            rowView["account"] = Txt_Account.Value.ToString().Trim();
            rowView["title"] = Txt_Title.Text.Trim();

            DBService.InsBookBase(rowView);
            rowView.EndEdit();
        }

        //編輯作業
        private void Operation_Edit()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            rowView["book"] = Txt_Book.Text.Trim();
            rowView["book_name"] = Txt_Book_Name.Text.Trim();
            rowView["book_type"] = Cmb_BookType.SelectedValue;
            rowView["bank"] = Txt_Bank.Value.ToString().Trim();
            rowView["bank_name"] = Txt_Bank_Name.Text.Trim();
            rowView["account"] = Txt_Account.Value.ToString().Trim();
            rowView["title"] = Txt_Title.Text.Trim();

            DBService.UpdBookBase(rowView);
            rowView.EndEdit();
            drv.Sort = drv.Sort == "" ? "[book]" : "";  //透過改變Sort強迫觸發IValueConverter，但Sort只有空白時可以給值，所以並不會被賦予空白
        }

        private void Operation_Delete()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            DBService.DelBookBase(rowView);
            rowView.Delete();
            rowView.EndEdit();
        }
    }

    /// <summary>
    /// Gride文字轉換器(顯示用，只實作Convert)
    /// </summary>
    public class BookBaseGrideConverter : IValueConverter
    {
        List<MapFile> Lst_BookType = DBService.GetMapFile("book_type");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataRowView r = value as DataRowView;
            string col = parameter as string;
            string result = "";

            if (col == "book_type")
            {
                result = Lst_BookType.Where(x => x.item == r.Row[col].ToString()).Select(s => s.item_name).FirstOrDefault();
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