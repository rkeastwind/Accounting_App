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
    /// Form_tra_trade.xaml 的互動邏輯
    /// </summary>
    public partial class Form_tra_trade : Window
    {
        FormState FormState = new FormState();

        List<MapFile> Lst_Tra = DBService.GetMapFile("1", new object[] { "1", "2" });  //只取收支
        List<MapFile> Lst_TraDtl = DBService.GetMapFile("2");
        List<MapFile> Lst_TraDtlIn = DBService.GetMapFile("2").Where(x => x.memo1 == "收入").ToList();
        List<MapFile> Lst_TraDtlOut = DBService.GetMapFile("2").Where(x => x.memo1 == "支出").ToList();
        List<MapFile> Lst_Acct = DBService.GetMapFile("AC");
        List<MapFile> Lst_AcctIn = DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "B").ToList();
        List<MapFile> Lst_AcctOut = DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "A").ToList();
        List<BookBase> Lst_BookBase = DBService.GetBookBase(false);

        DataTable DT_Main = DBService.QryTraMast("where 1=0");  //初始化

        public Form_tra_trade()
        {
            InitializeComponent();
            FormInitial();
        }

        public Form_tra_trade(string tl)
        {
            InitializeComponent();
            FormInitial();
            Title = tl;
        }

        private void FormInitial()
        {
            Qry_Action.ItemsSource = CommUtility.InsertBlankItem(Lst_Tra);
            Qry_ActionDtl.ItemsSource = CommUtility.InsertBlankItem(Lst_TraDtl);
            Cmb_Action.ItemsSource = new List<MapFile>(Lst_Tra);
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(FormStateS.Initial);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(FormStateS.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(FormStateS.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(FormStateS.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(FormStateS.ShowData); };
        }

        //查詢按鈕
        private void Btn_Qry_Click(object sender, RoutedEventArgs e)
        {
            lbStatusBar2.Text = "";
            string filter = "where 1=1";

            //查詢年月(月初到月底)
            DateTime QryDt = Qry_YearMonth.Value == null ? DateTime.Now : (DateTime)Qry_YearMonth.Value;
            string q_dt = QryDt.ToString("yyyy-MM") + "-01";  //月初
            filter += "\r\n  and " + $@"date(trade_dt) between date('{q_dt}','start of month') and date('{q_dt}','start of month','+1 month','-1 day')";

            //收支
            string q_action = Qry_Action.SelectedValue.ToString();
            filter += q_action != "" ? "\r\n  and " + $@"action = '{q_action}'" : "\r\n  and " + $@"action in ('1','2')";

            //交易方式
            string q_actiondtl = Qry_ActionDtl.SelectedValue.ToString();
            filter += q_actiondtl != "" ? "\r\n  and " + $@"action_dtl = '{q_actiondtl}'" : "";

            DT_Main = DBService.QryTraMast(filter);
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(FormStateS.ShowData);
            lbStatusBar2.Text = $"查詢成功，共{DT_Main.DefaultView.Count.ToString()}筆";
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
                MessageBox.Show($"{FormState.StateText}失敗，錯誤訊息：" + ex.Message.ToString());
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

            //小物件控制
            if (FormState.State == FormStateS.Initial || FormState.State == FormStateS.ShowData)
            {
                Txt_TradeNo.Text = "";
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == FormStateS.Add)
            {
                Txt_TradeNo.Text = "系統自動編號";
                Dtp_TradeDt.SelectedDate = CommUtility.GetNextProStartDt();
                Cmb_Action.SelectedIndex = 0;
                Cmb_ActionDtl.SelectedIndex = Cmb_AcctCode.SelectedIndex = 0;
                Txt_Amt.Value = 0;
                Txt_Memo.Text = "";
                UpdateMemoDef();
            }
            Dtp_TradeDt.IsHitTestVisible = Cmb_Action.IsHitTestVisible = Cmb_ActionDtl.IsHitTestVisible = !(FormState.State == FormStateS.Edit);  //新增後不可編輯
        }

        //查詢欄位連動(需填充空白選項)
        private void Qry_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Qry = Qry_Action.SelectedValue.ToString();
            if (Qry == "1")
            {
                Qry_ActionDtl.ItemsSource = CommUtility.InsertBlankItem(Lst_TraDtlIn);
            }
            else if (Qry == "2")
            {
                Qry_ActionDtl.ItemsSource = CommUtility.InsertBlankItem(Lst_TraDtlOut);
            }
            else
            {
                Qry_ActionDtl.ItemsSource = CommUtility.InsertBlankItem(Lst_TraDtl);
            }
        }

        //收支欄位連動
        private void Cmb_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Act = Cmb_Action.SelectedValue.ToString();
            if (Act == "1")
            {
                Cmb_ActionDtl.ItemsSource = new List<MapFile>(Lst_TraDtlIn);
                Cmb_AcctCode.ItemsSource = new List<MapFile>(Lst_AcctIn);
            }
            else if (Act == "2")
            {
                Cmb_ActionDtl.ItemsSource = new List<MapFile>(Lst_TraDtlOut);
                Cmb_AcctCode.ItemsSource = new List<MapFile>(Lst_AcctOut);
            }
        }

        //交易明細欄位連動(影響收入支出帳冊)
        private void Cmb_ActionDtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Act = Cmb_Action.SelectedValue.ToString();
            string Dtl = Cmb_ActionDtl.SelectedValue.ToString();
            if (Act == "1")  //收入
            {
                if (Dtl == "a")  //入現金
                    Cmb_BookIn.ItemsSource = Lst_BookBase.Where(x => x.book_type == "1").ToList();
                else if (Dtl == "b")  //入銀存
                    Cmb_BookIn.ItemsSource = Lst_BookBase.Where(x => x.book_type == "2").ToList();
                else  //其他
                    Cmb_BookIn.ItemsSource = new List<BookBase>(Lst_BookBase);
                Cmb_BookOut.ItemsSource = null;  //清空
            }
            else if (Act == "2")  //支出
            {
                if (Dtl == "1")  //現金
                    Cmb_BookOut.ItemsSource = Lst_BookBase.Where(x => x.book_type == "1").ToList();
                else if (Dtl == "2")  //銀行轉帳
                    Cmb_BookOut.ItemsSource = Lst_BookBase.Where(x => x.book_type == "2").ToList();
                else  //其他
                    Cmb_BookOut.ItemsSource = new List<BookBase>(Lst_BookBase);
                Cmb_BookIn.ItemsSource = null;  //清空
            }
            UpdateMemoDef();
        }

        //會計科目連動
        private void Cmb_AcctCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMemoDef();
        }

        //收入帳冊連動
        private void Cmb_BookIn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMemoDef();
        }

        //支出帳冊連動
        private void Cmb_BookOut_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMemoDef();
        }

        //取得Memo預設值
        private void UpdateMemoDef()
        {
            if (FormState.State != FormStateS.Add && FormState.State != FormStateS.Edit) return;
            Txt_Memo.Text = CommUtility.GetTraMastMemoDef(Cmb_Action.SelectedValue == null ? "" : Cmb_Action.SelectedValue.ToString(),
                Cmb_ActionDtl.SelectedValue == null ? "" : Cmb_ActionDtl.SelectedValue.ToString(),
                Cmb_AcctCode.SelectedValue == null ? "" : Cmb_AcctCode.SelectedValue.ToString(),
                Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue.ToString(),
                Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue.ToString());
        }

        //物件與Grid連動
        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Main.SelectedItems.Count > 0)
            {
                foreach (var r in DG_Main.SelectedItems)
                {
                    DataRowView drv = r as DataRowView;
                    Txt_TradeNo.Text = drv["trade_no"].ToString();
                    Dtp_TradeDt.SelectedDate = (DateTime)drv["trade_dt"];
                    Cmb_Action.SelectedValue = drv["action"];
                    Cmb_ActionDtl.SelectedValue = drv["action_dtl"];
                    Cmb_AcctCode.SelectedValue = drv["acct_code"];
                    Cmb_BookIn.SelectedValue = drv["acct_book_in"];
                    Cmb_BookOut.SelectedValue = drv["acct_book_out"];
                    Txt_Amt.Value = (decimal)drv["amt"];
                    Txt_Memo.Text = drv["memo"].ToString();
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
                if (!CommUtility.CheckIsPro((DateTime)Dtp_TradeDt.SelectedDate))
                {
                    MessageBox.Show($"{((DateTime)Dtp_TradeDt.SelectedDate).ToString("%M")}月已經結帳，" +
                        $"不可{ChangeState.GetDescriptionText()}", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                if (Dtp_TradeDt.SelectedDate == null)
                {
                    MessageBox.Show("交易日期必填", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (Txt_Amt.Value == 0)
                {
                    MessageBox.Show("金額不可為0", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            if (!CommUtility.CheckIsPro((DateTime)Dtp_TradeDt.SelectedDate))
            {
                MessageBox.Show($"{((DateTime)Dtp_TradeDt.SelectedDate).ToString("%M")}月已經結帳，" +
                    $"不可{FormState.GetDescriptionText()}", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        //新增作業
        private void Operation_Add()
        {
            string trade_no = CommUtility.GetNextTradeNo_TraMast((DateTime)Dtp_TradeDt.SelectedDate);
            DataView drv = DG_Main.ItemsSource as DataView;
            DataRowView rowView = drv.AddNew();
            rowView["trade_no"] = trade_no;
            rowView["trade_dt"] = (DateTime)Dtp_TradeDt.SelectedDate;
            rowView["action"] = Cmb_Action.SelectedValue;
            rowView["action_dtl"] = Cmb_ActionDtl.SelectedValue;
            rowView["acct_code"] = Cmb_AcctCode.SelectedValue;
            rowView["acct_book_in"] = Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue;
            rowView["acct_book_out"] = Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue;
            rowView["amt"] = Txt_Amt.Value;
            rowView["memo"] = Txt_Memo.Text.Trim();
            rowView["loguser"] = AppVar.UserName;
            rowView["logtime"] = DateTime.Now;

            DBService.InsTraMast(rowView);
            rowView.EndEdit();
        }

        //編輯作業
        private void Operation_Edit()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            rowView["trade_dt"] = (DateTime)Dtp_TradeDt.SelectedDate;
            rowView["action"] = Cmb_Action.SelectedValue;
            rowView["action_dtl"] = Cmb_ActionDtl.SelectedValue;
            rowView["acct_code"] = Cmb_AcctCode.SelectedValue;
            rowView["acct_book_in"] = Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue;
            rowView["acct_book_out"] = Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue;
            rowView["amt"] = Txt_Amt.Value;
            rowView["memo"] = Txt_Memo.Text.Trim();
            rowView["loguser"] = AppVar.UserName;
            rowView["logtime"] = DateTime.Now;

            DBService.UpdTraMast(rowView);
            rowView.EndEdit();
            drv.Sort = drv.Sort == "" ? "[trade_no]" : "";  //透過改變Sort強迫觸發IValueConverter，但Sort只有空白時可以給值，所以並不會被賦予空白
        }

        private void Operation_Delete()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            DBService.DelTraMast(rowView);
            rowView.Delete();
            rowView.EndEdit();
        }
    }

    /// <summary>
    /// Gride文字轉換器(顯示用，只實作Convert)
    /// </summary>
    public class TraTradeGrideConverter : IValueConverter
    {
        List<MapFile> Lst_Tra = DBService.GetMapFile("1", new object[] { "1", "2" });  //只取收支
        List<MapFile> Lst_TraDtl = DBService.GetMapFile("2");
        List<MapFile> Lst_Acct = DBService.GetMapFile("AC");
        List<BookBase> Lst_BookBase = DBService.GetBookBase(false);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DataRowView r = value as DataRowView;
            string col = parameter as string;
            string result = "";

            if (col == "action")
            {
                result = Lst_Tra.Where(x => x.item == r.Row[col].ToString()).Select(s => s.item_name).FirstOrDefault();
            }
            else if (col == "action_dtl")
            {
                result = Lst_TraDtl.Where(x => x.item == r.Row[col].ToString()).Select(s => s.item_name).FirstOrDefault();
            }
            else if (col == "acct_code")
            {
                result = Lst_Acct.Where(x => x.item == r.Row[col].ToString()).Select(s => s.item_name).FirstOrDefault();
            }
            else if (col == "acct_book_in" || col == "acct_book_out")
            {
                result = Lst_BookBase.Where(x => x.book == r.Row[col].ToString()).Select(s => s.book_name).FirstOrDefault();
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
