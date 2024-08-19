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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Runtime.InteropServices;
using Accounting_App.UserControls;

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

        string bookCloseNotice = "關閉帳冊條件：上次結帳日庫存為0，且大於上次結帳日沒有任何交易(含進/出)";
        string bookOpenNotice = "開啟帳冊條件：結帳日大於結束日才可以開啟";

        public Form_book_base()
        {
            InitializeComponent();
            FormInitial();
        }

        private void FormInitial()
        {
            DG_Main.ItemsSource = DBService.QryBookBase();
            BtnGroup_CRUD.permission = DBService.GetBasRolePermission(AppVar.User.role_id, AppVar.OpenMenuId);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(FormStateS.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(FormStateS.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(FormStateS.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(FormStateS.ShowData); };
            Txt_CloseNotice.Text = bookCloseNotice;
            Txt_OpenNotice.Text = bookOpenNotice;
            Refresh(FormStateS.Initial);
        }

        //新增、修改、刪除
        private void Btn_AED_Click(FormStateS st)
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
                    case FormStateS.Add:
                        Operation_Add();
                        break;
                    case FormStateS.Edit:
                        Operation_Edit();
                        break;
                    case FormStateS.Delete:
                        Operation_Delete();
                        break;
                }
                lbStatusBar2.Text = $"{FormState.StateText}成功";
            }
            catch (Exception ex)
            {
                new MessageBoxCustom($"{FormState.StateText}失敗，錯誤訊息：" + ex.Message.ToString(), "", MessageButtons.Ok, MessageType.Error).ShowDialog();
            }
            finally
            {
                Refresh(FormStateS.ShowData);
            }
        }

        //更新畫面與狀態
        private void Refresh(FormStateS IformStates)
        {
            FormState.State = IformStates;
            BtnGroup_CRUD.Refresh(IformStates);
            ObjectControl();
        }

        //畫面物件控制
        private void ObjectControl()
        {
            //大物件控制
            if (FormState.State == FormStateS.Initial || FormState.State == FormStateS.ShowData)
            {
                GpBox_Dtl.IsHitTestVisible = false;
                DG_Main.IsHitTestVisible = true;
            }
            else if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                GpBox_Dtl.IsHitTestVisible = true;
                DG_Main.IsHitTestVisible = false;
            }
            else if (FormState.State == FormStateS.Delete)
            {
                GpBox_Dtl.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }

            //小物件控制
            if (FormState.State == FormStateS.Add)  //先觸發改變選單，再觸發SelectionChanged
                Cmb_BookType.ItemsSource = Lst_BookType_NT;
            else
                Cmb_BookType.ItemsSource = Lst_BookType;

            if (FormState.State == FormStateS.Initial || FormState.State == FormStateS.ShowData)
            {
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == FormStateS.Add)
            {
                Cmb_BookType.SelectedIndex = 0;
                Txt_Book.Text = Txt_Book_Name.Text = Txt_Bank.Text = Txt_Bank_Name.Text = Txt_Account.Text = Txt_Title.Text = "";
                Dtp_OpenDate.SelectedDate = ProUtility.GetNextProStartDt();
                Chk_CloseDate.IsChecked = false;
                Chk_CloseDate_Checked(null, null);
                Chk_InQurey.IsChecked = true;
            }
            Txt_Book.IsHitTestVisible = Cmb_BookType.IsHitTestVisible = !(FormState.State == FormStateS.Edit);  //新增後不可編輯
        }

        //物件與Grid連動
        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Main.SelectedItems.Count > 0)
            {
                foreach (var r in DG_Main.SelectedItems)
                {
                    BookBase drv = r as BookBase;
                    Txt_Book.Text = drv.book;
                    Txt_Book_Name.Text = drv.book_name;
                    Cmb_BookType.SelectedValue = drv.book_type;
                    Txt_Bank.Text = drv.bank;
                    Txt_Bank_Name.Text = drv.bank_name;
                    Txt_Account.Text = drv.account;
                    Txt_Title.Text = drv.title;
                    Dtp_OpenDate.SelectedDate = drv.open_date;
                    Dtp_CloseDate.SelectedDate = drv.close_date;
                    Chk_CloseDate.IsChecked = drv.Is_Closed;
                    Chk_InQurey.IsChecked = drv.in_qurey;
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
        private bool CheckBeforeEdit(FormStateS ChangeState)
        {
            if (ChangeState == FormStateS.Delete)  //只卡刪除，修改仍可調整銀行資訊
            {
                if (!ProUtility.CheckBookIsPro(Txt_Book.Text))
                {
                    new MessageBoxCustom($"{Txt_Book.Text}帳冊已經結帳，" +
                        $"不可{ChangeState.GetDescriptionText()}", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState.State == FormStateS.Add)
            {
                var IsBookExists = DBService.QryBookBase($@"where book = '{Txt_Book.Text}'");
                if (IsBookExists.Count > 0)
                {
                    new MessageBoxCustom("帳冊代號已存在", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
            }
            if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                if (Txt_Book.Text == "")
                {
                    new MessageBoxCustom("帳冊代號必填", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
                if (!CheckClosedDate(Chk_CloseDate.IsChecked.Value))
                {
                    string msg = (Chk_CloseDate.IsChecked.Value) ? bookCloseNotice : bookOpenNotice;
                    new MessageBoxCustom(msg, "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
            }
            return true;
        }

        private bool CheckClosedDate(bool Mode)
        {
            string book = Txt_Book.Text;
            var NextProDt = ProUtility.GetNextProEndDt();
            var BookCloseDt = (DG_Main.SelectedItem as BookBase).close_date;  //因為畫面先動了，從Grid抓
            if (Mode)  //關閉檢核
            {
                var LastInvMast = DBService.GetProBaseBal(NextProDt).Where(x => x.acct_book == book).FirstOrDefault();
                var RangeTraMast = DBService.QryTraMast($"where date(trade_dt) >= date('{NextProDt.GetFullDate()}','start of month')").Where(x => x.acct_book_in == book || x.acct_book_out == book).ToList();
                if (LastInvMast.amt == 0 && RangeTraMast.Count == 0)  //上次結帳日庫存為0，且大於上次結帳日沒有任何交易(含進/出)
                    return true;
            }
            else  //開啟檢核
            {
                if (NextProDt > BookCloseDt)
                    return true;
            }
            return false;
        }

        //新增作業
        private void Operation_Add()
        {
            BookBase rowView = new BookBase()
            {
                book = Txt_Book.Text.Trim(),
                book_name = Txt_Book_Name.Text.Trim(),
                book_type = Convert.ToString(Cmb_BookType.SelectedValue),
                bank = Txt_Bank.Text.Trim(),
                bank_name = Txt_Bank_Name.Text.Trim(),
                account = Txt_Account.Text.Trim(),
                title = Txt_Title.Text.Trim(),
                open_date = Dtp_OpenDate.SelectedDate,
                close_date = Dtp_CloseDate.SelectedDate,
                in_qurey = Chk_InQurey.IsChecked ?? true,
                loguser = AppVar.User.user_id,
                logtime = DateTime.Now
            };

            rowView.InsertDB();
            (DG_Main.ItemsSource as List<BookBase>).Add(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        //編輯作業
        private void Operation_Edit()
        {
            var rowView = DG_Main.SelectedItem as BookBase;
            rowView.book = Txt_Book.Text.Trim();
            rowView.book_name = Txt_Book_Name.Text.Trim();
            rowView.book_type = Convert.ToString(Cmb_BookType.SelectedValue);
            rowView.bank = Txt_Bank.Text.Trim();
            rowView.bank_name = Txt_Bank_Name.Text.Trim();
            rowView.account = Txt_Account.Text.Trim();
            rowView.title = Txt_Title.Text.Trim();
            rowView.open_date = Dtp_OpenDate.SelectedDate;
            rowView.close_date = Dtp_CloseDate.SelectedDate;
            rowView.in_qurey = Chk_InQurey.IsChecked ?? true;
            rowView.loguser = AppVar.User.user_id;
            rowView.logtime = DateTime.Now;

            rowView.UpdateDB();
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        private void Operation_Delete()
        {
            var rowView = DG_Main.SelectedItem as BookBase;
            rowView.DeleteDB();
            (DG_Main.ItemsSource as List<BookBase>).Remove(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        private void Chk_CloseDate_Checked(object sender, RoutedEventArgs e)
        {
            if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                if (Chk_CloseDate.IsChecked == true)
                    Dtp_CloseDate.SelectedDate = ProUtility.GetLastProEndDt();  //上一結帳日
                else
                    Dtp_CloseDate.SelectedDate = null;
            }
        }
    }
}