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
        FormState FormState = new FormState();

        List<MapFile> Lst_BookType = DBService.GetMapFile("book_type");
        List<MapFile> Lst_BookType_NT = DBService.GetMapFile("book_type").Where(x => x.item != "0").ToList();

        DataTable DT_Main = DBService.QryBookBase();  //初始化


        public Form_book_base()
        {
            InitializeComponent();
            FormInitial();
        }

        public Form_book_base(string tl)
        {
            InitializeComponent();
            FormInitial();
            Title = tl;
        }

        private void FormInitial()
        {
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(EnumFormStates.Initial);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(EnumFormStates.ShowData); };
        }

        //新增、修改、刪除
        private void Btn_AED_Click(EnumFormStates st)
        {
            if (!CheckBeforeEdit(st)) return;
            Refresh(st);
        }

        //儲存按鈕
        private void Btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBefoeSave()) return;  //檢核
            try
            {
                switch (FormState.State)
                {
                    case EnumFormStates.Add:
                        Operation_Add();
                        break;
                    case EnumFormStates.Edit:
                        Operation_Edit();
                        break;
                    case EnumFormStates.Delete:
                        Operation_Delete();
                        break;
                }
                lbStatusBar2.Text = $"{FormState.StateText}成功";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{FormState.StateText}失敗，錯誤訊息：" + ex.Message.ToString());
            }
            finally
            {
                Refresh(EnumFormStates.ShowData);
            }
        }

        //更新畫面與狀態
        private void Refresh(EnumFormStates IformStates)
        {
            FormState.State = IformStates;
            BtnGroup_CRUD.Refresh(IformStates);
            ObjectControl();
        }

        //畫面物件控制
        private void ObjectControl()
        {
            //大物件控制
            if (FormState.State == EnumFormStates.Initial || FormState.State == EnumFormStates.ShowData)
            {
                GpBox_Dtl.IsHitTestVisible = false;
                DG_Main.IsHitTestVisible = true;
            }
            else if (FormState.State == EnumFormStates.Add || FormState.State == EnumFormStates.Edit)
            {
                GpBox_Dtl.IsHitTestVisible = true;
                DG_Main.IsHitTestVisible = false;
            }
            else if (FormState.State == EnumFormStates.Delete)
            {
                GpBox_Dtl.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }

            //小物件控制
            if (FormState.State == EnumFormStates.Add)  //先觸發改變選單，再觸發SelectionChanged
                Cmb_BookType.ItemsSource = Lst_BookType_NT;
            else
                Cmb_BookType.ItemsSource = Lst_BookType;

            if (FormState.State == EnumFormStates.Initial || FormState.State == EnumFormStates.ShowData)
            {
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == EnumFormStates.Add)
            {
                Cmb_BookType.SelectedIndex = 0;
                Txt_Book.Text = Txt_Book_Name.Text = Txt_Bank.Text = Txt_Bank_Name.Text = Txt_Account.Text = Txt_Title.Text = "";
            }
            Txt_Book.IsHitTestVisible = Cmb_BookType.IsHitTestVisible = !(FormState.State == EnumFormStates.Edit);  //新增後不可編輯
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
            BtnGroup_CRUD.CanEditDelete = (DG_Main.SelectedItems.Count == 1);

            if (Txt_Book.Text == "Total")  //總帳冊不可修改
                BtnGroup_CRUD.CanEditDelete = false;
        }

        /// <summary>
        /// 修改前檢核
        /// </summary>
        /// <param name="ChangeState">要改變的State</param>
        /// <returns></returns>
        private bool CheckBeforeEdit(EnumFormStates ChangeState)
        {
            if (ChangeState == EnumFormStates.Delete)  //只卡刪除，修改仍可調整銀行資訊
            {
                if (!CommUtility.CheckBookIsPro(Txt_Book.Text))
                {
                    MessageBox.Show($"{Txt_Book.Text}帳冊已經結帳，" +
                        $"不可{Enum.GetName(typeof(EnumFormStatesText), ChangeState)}", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState.State == EnumFormStates.Add)
            {
                DataTable IsBookExists = DBService.QryBookBase($@"where book = '{Txt_Book.Text}'");
                if (IsBookExists.Rows.Count > 0)
                {
                    MessageBox.Show("帳冊代號已存在", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            if (FormState.State == EnumFormStates.Add || FormState.State == EnumFormStates.Edit)
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