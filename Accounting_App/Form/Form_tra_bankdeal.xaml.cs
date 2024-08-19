using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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
using Accounting_App.DTO;
using Accounting_App.UserControls;
using Accounting_App.Utilities;

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_tra_bankdeal.xaml 的互動邏輯
    /// </summary>
    public partial class Form_tra_bankdeal : Window
    {
        FormState FormState = new FormState();

        List<MapFile> Lst_Tra = DBService.GetMapFile("1", new object[] { "3", "4", "5", "6" });  //3.銀行存入、4.銀行提領、5.銀行調撥、6.現金調撥

        //金額Base，運算用
        decimal C_BookOutAmt = 0;
        decimal C_BookInAmt = 0;

        public Form_tra_bankdeal()
        {
            InitializeComponent();
            FormInitial();
        }

        private void FormInitial()
        {
            Cmb_Action.ItemsSource = new List<MapFile>(Lst_Tra);
            DG_Main.ItemsSource = DBService.QryTraMast("where 1=0");  //初始化;
            BtnGroup_CRUD.permission = DBService.GetBasRolePermission(AppVar.User.role_id, AppVar.OpenMenuId);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(FormStateS.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(FormStateS.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(FormStateS.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(FormStateS.ShowData); };
            Refresh(FormStateS.Initial);
        }

        private void Btn_Qry_Click(object sender, RoutedEventArgs e)
        {
            lbStatusBar2.Text = "";
            string filter = "where 1=1";

            //查詢年月(月初到月底)
            DateTime QryDt = Qry_YearMonth.SelectedDate == null ? DateTime.Now : (DateTime)Qry_YearMonth.SelectedDate;
            string q_dt = QryDt.ToString("yyyy-MM") + "-01";  //月初
            filter += "\r\n  and " + $@"date(trade_dt) between date('{q_dt}','start of month') and date('{q_dt}','start of month','+1 month','-1 day')";
            //交易方式
            filter += "\r\n  and " + $@"action in ('3','4','5','6')";

            var DT_Main = DBService.QryTraMast(filter);
            DG_Main.ItemsSource = DT_Main;
            Refresh(FormStateS.ShowData);
            lbStatusBar2.Text = $"查詢成功，共{DT_Main.Count}筆";
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
            if (!CheckBeforeSave()) return;  //檢核
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
                GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = true;
            }
            else if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                GpBox_Dtl.IsHitTestVisible = true;
                GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }
            else if (FormState.State == FormStateS.Delete)
            {
                GpBox_Dtl.IsHitTestVisible = GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }

            //小物件控制(先還原Amt再對帳冊動作)
            if (FormState.State == FormStateS.Initial || FormState.State == FormStateS.ShowData)
            {
                Cmb_BookIn.ItemsSource = DBService.GetBookBase();
                Cmb_BookOut.ItemsSource = DBService.GetBookBase();
                Txt_TradeNo.Text = "";
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == FormStateS.Add)
            {
                Txt_TradeNo.Text = "系統自動編號";
                Dtp_TradeDt.SelectedDate = ProUtility.GetNextProStartDt();
                Cmb_Action.SelectedIndex = 0;
                Txt_Amt.Value = 0;
                Txt_Memo.Text = "";
                Cmb_Action_SelectionChanged(null, null);
                Cmb_BookIn.SelectedIndex = Cmb_BookOut.SelectedIndex = 0;
            }
            else if (FormState.State == FormStateS.Edit)
            {
                object BBIn_SelectValue = Cmb_BookIn.SelectedValue;
                object BBOut_SelectValue = Cmb_BookOut.SelectedValue;
                Cmb_Action_SelectionChanged(null, null);
                //重新給值(因為ItemsSource換掉會變)
                Cmb_BookIn.SelectedValue = BBIn_SelectValue;
                Cmb_BookOut.SelectedValue = BBOut_SelectValue;
            }
            Dtp_TradeDt.IsHitTestVisible = Cmb_Action.IsHitTestVisible = Cmb_BookIn.IsHitTestVisible = Cmb_BookOut.IsHitTestVisible = !(FormState.State == FormStateS.Edit);  //新增後不可編輯
        }

        private void Dtp_TradeDt_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormState.State != FormStateS.Add) return;  //新增才用物件控制
            C_BookOutAmt = GetBookAmt(Cmb_BookOut.SelectedItem as BookBase);
            C_BookInAmt = GetBookAmt(Cmb_BookIn.SelectedItem as BookBase);
            Txt_Amt_ValueChanged(Txt_Amt, null);
            CheckBookSelect();
        }

        //交易方式欄位連動
        private void Cmb_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FormState.State != FormStateS.Add && FormState.State != FormStateS.Edit)
                return;

            List<BookBase> BBIn = new List<BookBase>();
            List<BookBase> BBOut = new List<BookBase>();
            if (FormState.State == FormStateS.Add)  //要排除已關閉的帳冊，但Edit情況會有舊資料，自身帳冊要保留
            {
                BBIn = DBService.GetBookBaseForTrade();
                BBOut = DBService.GetBookBaseForTrade();
            }
            else
            {
                BBIn = DBService.GetBookBaseForTrade((DG_Main.SelectedItem as TraMast).acct_book_in);
                BBOut = DBService.GetBookBaseForTrade((DG_Main.SelectedItem as TraMast).acct_book_out);
            }

            string Act = Convert.ToString(Cmb_Action.SelectedValue);
            Cmb_BookIn.ItemsSource = (new List<string> { "3", "5" }).Contains(Act) ? BBIn.Where(x => x.book_type == "2").ToList() : BBIn.Where(x => x.book_type == "1").ToList();
            Cmb_BookOut.ItemsSource = (new List<string> { "4", "5" }).Contains(Act) ? BBOut.Where(x => x.book_type == "2").ToList() : BBOut.Where(x => x.book_type == "1").ToList();
            UpdateMemoDef();
        }

        private void Cmb_BookOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //帶出文字
            Txt_BookOutInfo.Text = GetBookInfo(Cmb_BookOut.SelectedItem as BookBase);
            UpdateMemoDef();

            if (FormState.State != FormStateS.Add) return;  //新增才用物件控制
            //取得庫存
            C_BookOutAmt = GetBookAmt(Cmb_BookOut.SelectedItem as BookBase);
            Txt_Amt_ValueChanged(Txt_Amt, null);
            CheckBookSelect();
        }

        private void Cmb_BookIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //帶出文字
            Txt_BookInInfo.Text = GetBookInfo(Cmb_BookIn.SelectedItem as BookBase);
            UpdateMemoDef();

            if (FormState.State != FormStateS.Add) return;  //新增才用物件控制
            //取得庫存
            C_BookInAmt = GetBookAmt(Cmb_BookIn.SelectedItem as BookBase);
            Txt_Amt_ValueChanged(Txt_Amt, null);
            CheckBookSelect();
        }

        //取得Memo預設值
        private void UpdateMemoDef()
        {
            if (FormState.State != FormStateS.Add && FormState.State != FormStateS.Edit) return;
            Txt_Memo.Text = ProUtility.GetTraMastMemoDef(Convert.ToString(Cmb_Action.SelectedValue),
                "0",
                "",
                Convert.ToString(Cmb_BookIn.SelectedValue),
                Convert.ToString(Cmb_BookOut.SelectedValue));
        }

        private void CheckBookSelect()
        {
            if (Cmb_BookOut.SelectedValue != null && Cmb_BookIn.SelectedValue != null)
                Txt_Book_Msg.Text = (Convert.ToString(Cmb_BookOut.SelectedValue) == Convert.ToString(Cmb_BookIn.SelectedValue)) ? "轉出帳冊與轉入帳冊不可相同" : "";
            Txt_Amt_Msg.Text = (Txt_BookInAmt.Value < 0 || Txt_BookOutAmt.Value < 0) ? "出入帳冊庫存不可小於0" : "";
        }


        private string GetBookInfo(BookBase Book)
        {
            return (Book.bank != "" ? Book.bank + "\r\n" : "")
                + (Book.bank_name != "" ? Book.bank_name + "\r\n" : "")
                + (Book.account != "" ? Book.account + "\r\n" : "")
                + (Book.title != "" ? Book.title : "");
        }

        /// <summary>
        /// 取得庫存(上個月底庫存+累計交易)
        /// </summary>
        /// <returns></returns>
        private decimal GetBookAmt(BookBase Book)
        {
            //查詢年月
            DateTime QryDt = Dtp_TradeDt.SelectedDate == null ? DateTime.Now : (DateTime)Dtp_TradeDt.SelectedDate;
            string q_dt = QryDt.GetFullDate();

            //庫存_上個月底
            string filter_bal = "\r\n" + $@"where date(trade_dt) = date('{q_dt}','start of month','-1 day') and acct_book = '{Book.book}'";
            var T_LastInv = DBService.QryInvMast(filter_bal);
            decimal LastInv = T_LastInv.Count == 0 ? 0 : T_LastInv.FirstOrDefault().amt;

            //交易_月初到今日
            string filter_tra = "\r\n" + $@"where date(trade_dt) between date('{q_dt}','start of month') and '{q_dt}'";
            List<TraMast> T_Tra_P = DBService.QryTraMast(filter_tra + $@" and acct_book_in = '{Book.book}'");  //加項
            List<TraMast> T_Tra_M = DBService.QryTraMast(filter_tra + $@" and acct_book_out = '{Book.book}'");  //減項
            decimal Tra_P = T_Tra_P.Sum(x => x.amt);
            decimal Tra_M = T_Tra_M.Sum(x => x.amt);

            decimal result = LastInv + Tra_P - Tra_M;
            return result;
        }

        private void Txt_Amt_ValueChanged(object sender, TextChangedEventArgs e)
        {
            if (Txt_BookOutAmt != null && Txt_BookInAmt != null)
            {
                Txt_BookOutAmt.Value = C_BookOutAmt - Txt_Amt.Value;
                Txt_BookInAmt.Value = C_BookInAmt + Txt_Amt.Value;
                CheckBookSelect();
            }
        }

        //物件與Grid連動
        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Main.SelectedItems.Count > 0)
            {
                foreach (var r in DG_Main.SelectedItems)
                {
                    TraMast drv = r as TraMast;
                    Txt_TradeNo.Text = drv.trade_no;
                    Dtp_TradeDt.SelectedDate = drv.trade_dt;
                    Cmb_Action.SelectedValue = drv.action;
                    Cmb_BookOut.SelectedValue = drv.acct_book_out;
                    Cmb_BookIn.SelectedValue = drv.acct_book_in;
                    Txt_Amt.Value = drv.amt;
                    Txt_Memo.Text = drv.memo;

                    //新增跟修改的Base不同，分開寫才不會亂掉
                    //新增：Base為當日累計 => 用物件的SelectChange控制
                    //修改：Base為當日累計+-該筆交易 => 用RowSelectChange控制
                    C_BookOutAmt = GetBookAmt(Cmb_BookOut.SelectedItem as BookBase) + Txt_Amt.Value;
                    C_BookInAmt = GetBookAmt(Cmb_BookIn.SelectedItem as BookBase) - Txt_Amt.Value;
                    Txt_Amt_ValueChanged(Txt_Amt, null);
                }
            }

            //按鈕控制(選擇一列時才允許Edit和Delete)
            BtnGroup_CRUD.CanEditDelete = (DG_Main.SelectedItems.Count == 1);
        }

        /// <summary>
        /// 修改前檢核
        /// </summary>
        /// <param name="ChangeState">要改變的State</param>
        /// <returns></returns>
        private bool CheckBeforeEdit(FormStateS ChangeState)
        {
            if (ChangeState == FormStateS.Edit || ChangeState == FormStateS.Delete)
            {
                if (!ProUtility.CheckIsPro((DateTime)Dtp_TradeDt.SelectedDate))
                {
                    new MessageBoxCustom($"{((DateTime)Dtp_TradeDt.SelectedDate).ToString("%M")}月已經結帳，" +
                        $"不可{ChangeState.GetDescriptionText()}", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
            }
            return true;
        }

        //儲存檢核
        private bool CheckBeforeSave()
        {
            if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                if (Dtp_TradeDt.SelectedDate == null)
                {
                    new MessageBoxCustom("交易日期必填", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
                if (Txt_Amt.Value == 0)
                {
                    new MessageBoxCustom("金額不可為0", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
                if (Convert.ToString(Cmb_BookOut.SelectedValue) == Convert.ToString(Cmb_BookIn.SelectedValue))
                {
                    new MessageBoxCustom("轉出帳冊與轉入帳冊不可相同", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
                if (Txt_BookInAmt.Value < 0 || Txt_BookOutAmt.Value < 0)
                {
                    new MessageBoxCustom("出入帳冊庫存不可小於0", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                    return false;
                }
            }
            if (!ProUtility.CheckIsPro((DateTime)Dtp_TradeDt.SelectedDate))
            {
                new MessageBoxCustom($"{((DateTime)Dtp_TradeDt.SelectedDate).ToString("%M")}月已經結帳，" +
                    $"不可{FormState.GetDescriptionText()}", "檢核失敗", MessageButtons.Ok, MessageType.Warning).ShowDialog();
                return false;
            }
            return true;
        }

        //新增作業
        private void Operation_Add()
        {
            string trade_no = ProUtility.GetNextTradeNo_TraMast((DateTime)Dtp_TradeDt.SelectedDate);
            TraMast rowView = new TraMast()
            {
                trade_no = trade_no,
                trade_dt = (DateTime)Dtp_TradeDt.SelectedDate,
                action = Convert.ToString(Cmb_Action.SelectedValue),
                action_dtl = "0",
                acct_code = "",
                acct_book_in = Convert.ToString(Cmb_BookIn.SelectedValue),
                acct_book_out = Convert.ToString(Cmb_BookOut.SelectedValue),
                amt = Txt_Amt.Value,
                memo = Txt_Memo.Text.Trim(),
                loguser = AppVar.User.user_id,
                logtime = DateTime.Now
            };

            rowView.InsertDB();
            (DG_Main.ItemsSource as List<TraMast>).Add(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        //編輯作業
        private void Operation_Edit()
        {
            var rowView = DG_Main.SelectedItem as TraMast;
            rowView.trade_dt = (DateTime)Dtp_TradeDt.SelectedDate;
            rowView.action = Convert.ToString(Cmb_Action.SelectedValue);
            rowView.action_dtl = "0";
            rowView.acct_code = "";
            rowView.acct_book_in = Convert.ToString(Cmb_BookIn.SelectedValue);
            rowView.acct_book_out = Convert.ToString(Cmb_BookOut.SelectedValue);
            rowView.amt = Txt_Amt.Value;
            rowView.memo = Txt_Memo.Text.Trim();
            rowView.loguser = AppVar.User.user_id;
            rowView.logtime = DateTime.Now;

            rowView.UpdateDB();
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        private void Operation_Delete()
        {
            var rowView = DG_Main.SelectedItem as TraMast;
            rowView.DeleteDB();
            (DG_Main.ItemsSource as List<TraMast>).Remove(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }
    }
}
