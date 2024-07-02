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
        List<MapFile> Lst_AcctIn = DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "B").ToList();
        List<MapFile> Lst_AcctOut = DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "A").ToList();
        List<BookBase> Lst_BookBase = DBService.GetBookBase(false);

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
            DG_Main.ItemsSource = DBService.QryTraMast("where 1=0");  //初始化
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
            string q_action = Convert.ToString(Qry_Action.SelectedValue);
            filter += q_action != "" ? "\r\n  and " + $@"action = '{q_action}'" : "\r\n  and " + $@"action in ('1','2')";

            //交易方式
            string q_actiondtl = Convert.ToString(Qry_ActionDtl.SelectedValue);
            filter += q_actiondtl != "" ? "\r\n  and " + $@"action_dtl = '{q_actiondtl}'" : "";

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
            string Qry = Convert.ToString(Qry_Action.SelectedValue);
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
            string Act = Convert.ToString(Cmb_Action.SelectedValue);
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
            string Act = Convert.ToString(Cmb_Action.SelectedValue);
            string Dtl = Convert.ToString(Cmb_ActionDtl.SelectedValue);
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
            Txt_Memo.Text = CommUtility.GetTraMastMemoDef(Convert.ToString(Cmb_Action.SelectedValue),
                Convert.ToString(Cmb_ActionDtl.SelectedValue),
                Convert.ToString(Cmb_AcctCode.SelectedValue),
                Convert.ToString(Cmb_BookIn.SelectedValue),
                Convert.ToString(Cmb_BookOut.SelectedValue));
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
                    Cmb_ActionDtl.SelectedValue = drv.action_dtl;
                    Cmb_AcctCode.SelectedValue = drv.acct_code;
                    Cmb_BookIn.SelectedValue = drv.acct_book_in;
                    Cmb_BookOut.SelectedValue = drv.acct_book_out;
                    Txt_Amt.Value = drv.amt;
                    Txt_Memo.Text = drv.memo;
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
            TraMast rowView = new TraMast()
            {
                trade_no = trade_no,
                trade_dt = (DateTime)Dtp_TradeDt.SelectedDate,
                action = Convert.ToString(Cmb_Action.SelectedValue),
                action_dtl = Convert.ToString(Cmb_ActionDtl.SelectedValue),
                acct_code = Convert.ToString(Cmb_AcctCode.SelectedValue),
                acct_book_in = Convert.ToString(Cmb_BookIn.SelectedValue),
                acct_book_out = Convert.ToString(Cmb_BookOut.SelectedValue),
                amt = Txt_Amt.Value.Value,
                memo = Txt_Memo.Text.Trim(),
                loguser = AppVar.UserName,
                logtime = DateTime.Now,
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
            rowView.action_dtl = Convert.ToString(Cmb_ActionDtl.SelectedValue);
            rowView.acct_code = Convert.ToString(Cmb_AcctCode.SelectedValue);
            rowView.acct_book_in = Convert.ToString(Cmb_BookIn.SelectedValue);
            rowView.acct_book_out = Convert.ToString(Cmb_BookOut.SelectedValue);
            rowView.amt = Txt_Amt.Value.Value;
            rowView.memo = Txt_Memo.Text.Trim();
            rowView.loguser = AppVar.UserName;
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
