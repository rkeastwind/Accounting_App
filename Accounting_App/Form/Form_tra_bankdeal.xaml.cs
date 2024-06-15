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
        List<BookBase> Lst_BookBase = DBService.GetBookBase(false);

        DataTable DT_Main = DBService.QryTraMast("where 1=0");  //初始化

        //金額Base，運算用
        decimal C_BookOutAmt = 0;
        decimal C_BookInAmt = 0;

        public Form_tra_bankdeal()
        {
            InitializeComponent();
            FormInitial();
        }

        public Form_tra_bankdeal(string tl)
        {
            InitializeComponent();
            FormInitial();
            Title = tl;
        }

        private void FormInitial()
        {
            Cmb_Action.ItemsSource = new List<MapFile>(Lst_Tra);
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(FormStateS.Initial);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(FormStateS.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(FormStateS.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(FormStateS.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(FormStateS.ShowData); };
        }

        private void Btn_Qry_Click(object sender, RoutedEventArgs e)
        {
            lbStatusBar2.Text = "";
            string filter = "where 1=1";

            //查詢年月(月初到月底)
            DateTime QryDt = Qry_YearMonth.Value == null ? DateTime.Now : (DateTime)Qry_YearMonth.Value;
            string q_dt = QryDt.ToString("yyyy-MM") + "-01";  //月初
            filter += "\r\n  and " + $@"date(trade_dt) between date('{q_dt}','start of month') and date('{q_dt}','start of month','+1 month','-1 day')";
            //交易方式
            filter += "\r\n  and " + $@"action in ('3','4','5','6')";

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

            //小物件控制(先還原Amt再對帳冊動作)
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
                Txt_Amt.Value = 0;
                Txt_Memo.Text = "";
                UpdateMemoDef();
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
            string Act = Cmb_Action.SelectedValue.ToString();

            Cmb_BookIn.ItemsSource = (new List<string> { "3", "5" }).Contains(Act) ? Lst_BookBase.Where(x => x.book_type == "2").ToList() : Lst_BookBase.Where(x => x.book_type == "1").ToList();
            Cmb_BookOut.ItemsSource = (new List<string> { "4", "5" }).Contains(Act) ? Lst_BookBase.Where(x => x.book_type == "2").ToList() : Lst_BookBase.Where(x => x.book_type == "1").ToList();
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
            Txt_Memo.Text = CommUtility.GetTraMastMemoDef(Cmb_Action.SelectedValue == null ? "" : Cmb_Action.SelectedValue.ToString(),
                "0",
                "",
                Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue.ToString(),
                Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue.ToString());
        }

        private void CheckBookSelect()
        {
            if (Cmb_BookOut.SelectedValue != null && Cmb_BookIn.SelectedValue != null)
                Txt_Book_Msg.Text = (Cmb_BookOut.SelectedValue.ToString() == Cmb_BookIn.SelectedValue.ToString()) ? "轉出帳冊與轉入帳冊不可相同" : "";
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
            string q_dt = QryDt.ToString("yyyy-MM-dd");

            //庫存_上個月底
            string filter_bal = "\r\n" + $@"where date(trade_dt) = date('{q_dt}','start of month','-1 day') and acct_book = '{Book.book}'";
            DataTable T_LastInv = DBService.QryInvMast(filter_bal);
            decimal LastInv = T_LastInv.Rows.Count == 0 ? 0 : (decimal)T_LastInv.AsEnumerable().FirstOrDefault()["amt"];

            //交易_月初到今日
            string filter_tra = "\r\n" + $@"where date(trade_dt) between date('{q_dt}','start of month') and '{q_dt}'";
            DataTable T_Tra_P = DBService.QryTraMast(filter_tra + $@" and acct_book_in = '{Book.book}'");  //加項
            DataTable T_Tra_M = DBService.QryTraMast(filter_tra + $@" and acct_book_out = '{Book.book}'");  //減項
            decimal Tra_P = T_Tra_P.Rows.Count == 0 ? 0 : (decimal)T_Tra_P.Compute("SUM(amt)", "");
            decimal Tra_M = T_Tra_M.Rows.Count == 0 ? 0 : (decimal)T_Tra_M.Compute("SUM(amt)", "");

            decimal result = LastInv + Tra_P - Tra_M;
            return result;
        }

        private void Txt_Amt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
                    DataRowView drv = r as DataRowView;
                    Txt_TradeNo.Text = drv["trade_no"].ToString();
                    Dtp_TradeDt.SelectedDate = (DateTime)drv["trade_dt"];
                    Cmb_Action.SelectedValue = drv["action"];
                    Cmb_BookOut.SelectedValue = drv["acct_book_out"];
                    Cmb_BookIn.SelectedValue = drv["acct_book_in"];
                    Txt_Amt.Value = (decimal)drv["amt"];
                    Txt_Memo.Text = drv["memo"].ToString();

                    //新增跟修改的Base不同，分開寫才不會亂掉
                    //新增：Base為當日累計 => 用物件的SelectChange控制
                    //修改：Base為當日累計+-該筆交易 => 用RowSelectChange控制
                    C_BookOutAmt = GetBookAmt(Cmb_BookOut.SelectedItem as BookBase) + Txt_Amt.Value.Value;
                    C_BookInAmt = GetBookAmt(Cmb_BookIn.SelectedItem as BookBase) - Txt_Amt.Value.Value;
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
        private bool CheckBeforeSave()
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
                if (Cmb_BookOut.SelectedValue.ToString() == Cmb_BookIn.SelectedValue.ToString())
                {
                    MessageBox.Show("轉出帳冊與轉入帳冊不可相同", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
                if (Txt_BookInAmt.Value < 0 || Txt_BookOutAmt.Value < 0)
                {
                    MessageBox.Show("出入帳冊庫存不可小於0", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            rowView["action_dtl"] = 0;
            rowView["acct_code"] = "";
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
            rowView["action_dtl"] = 0;
            rowView["acct_code"] = "";
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
    public class TraBankDealGrideConverter : IValueConverter
    {
        List<MapFile> Lst_Tra = DBService.GetMapFile("1", new object[] { "3", "4", "5", "6" });  //3.銀行存入、4.銀行提領、5.銀行調撥、6.現金調撥
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
