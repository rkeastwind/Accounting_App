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
    /// Form_tra_mast_memodef.xaml 的互動邏輯
    /// </summary>
    public partial class Form_tra_mast_memodef : Window
    {
        FormState FormState = new FormState();

        //都加上空白全選選項
        List<MapFile> Lst_Tra = CommUtility.InsertBlankItem(DBService.GetMapFile("1"));
        List<MapFile> Lst_TraDtl = CommUtility.InsertBlankItem(DBService.GetMapFile("2"));
        List<MapFile> Lst_TraDtlIn = CommUtility.InsertBlankItem(DBService.GetMapFile("2").Where(x => x.memo1 == "收入").ToList());
        List<MapFile> Lst_TraDtlOut = CommUtility.InsertBlankItem(DBService.GetMapFile("2").Where(x => x.memo1 == "支出").ToList());
        List<MapFile> Lst_Acct = CommUtility.InsertBlankItem(DBService.GetMapFile("AC"));
        List<MapFile> Lst_AcctIn = CommUtility.InsertBlankItem(DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "B").ToList());
        List<MapFile> Lst_AcctOut = CommUtility.InsertBlankItem(DBService.GetMapFile("AC").Where(x => x.item.Substring(0, 1) == "A").ToList());
        List<BookBase> Lst_BookBase = CommUtility.InsertBlankItem(DBService.GetBookBase(false));

        DataTable DT_Main = DBService.QryTraMastMemoDef("where 1=1");  //初始化

        List<string> Lst_BankDealTra = new List<string>() { "3", "4", "5", "6" };

        public Form_tra_mast_memodef()
        {
            InitializeComponent();
            FormInitial();
        }

        public Form_tra_mast_memodef(string tl)
        {
            InitializeComponent();
            FormInitial();
            Title = tl;
        }

        private void FormInitial()
        {
            Qry_Action.ItemsSource = new List<MapFile>(Lst_Tra);
            Qry_Action.SelectedValue = "";  //觸發Select事件

            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(EnumFormStates.Initial);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(EnumFormStates.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(EnumFormStates.ShowData); };
        }

        //查詢按鈕
        private void Btn_Qry_Click(object sender, RoutedEventArgs e)
        {
            lbStatusBar2.Text = "";
            string filter = "where 1=1";

            string q = "";
            q = (Qry_Action.SelectedValue == null) ? "" : Qry_Action.SelectedValue.ToString();
            if (q != "") filter += $"\r\n and action = '{q}'";

            q = (Qry_ActionDtl.SelectedValue == null) ? "" : Qry_ActionDtl.SelectedValue.ToString();
            if (q != "") filter += $"\r\n and action_dtl = '{q}'";

            q = (Qry_AcctCode.SelectedValue == null) ? "" : Qry_AcctCode.SelectedValue.ToString();
            if (q != "") filter += $"\r\n and acct_code = '{q}'";

            q = (Qry_BookIn.SelectedValue == null) ? "" : Qry_BookIn.SelectedValue.ToString();
            if (q != "") filter += $"\r\n and acct_book_in = '{q}'";

            q = (Qry_BookOut.SelectedValue == null) ? "" : Qry_BookOut.SelectedValue.ToString();
            if (q != "") filter += $"\r\n and acct_book_out = '{q}'";

            DT_Main = DBService.QryTraMastMemoDef(filter);
            DG_Main.ItemsSource = DT_Main.DefaultView;
            Refresh(EnumFormStates.ShowData);
            lbStatusBar2.Text = $"查詢成功，共{DT_Main.DefaultView.Count.ToString()}筆";
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
                GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = true;
            }
            else if (FormState.State == EnumFormStates.Add || FormState.State == EnumFormStates.Edit)
            {
                GpBox_Dtl.IsHitTestVisible = true;
                GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }
            else if (FormState.State == EnumFormStates.Delete)
            {
                GpBox_Dtl.IsHitTestVisible = GpBox_Qry.IsHitTestVisible = DG_Main.IsHitTestVisible = false;
            }

            //小物件控制
            if (FormState.State != EnumFormStates.Edit && FormState.State != EnumFormStates.Delete)
            {
                Cmb_Action.ItemsSource = new List<MapFile>(Lst_Tra);
                Cmb_Action.SelectedValue = "";  //觸發Select事件
            }

            if (FormState.State == EnumFormStates.Initial || FormState.State == EnumFormStates.ShowData)
            {
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == EnumFormStates.Add)
            {
                Cmb_Action.SelectedIndex = Cmb_ActionDtl.SelectedIndex = Cmb_AcctCode.SelectedIndex = 0;
                Cmb_BookIn.SelectedIndex = Cmb_BookOut.SelectedIndex = 0;
                Txt_MemoDef.Text = "";
            }
            Cmb_Action.IsHitTestVisible = Cmb_ActionDtl.IsHitTestVisible = Cmb_AcctCode.IsHitTestVisible
                = Cmb_BookIn.IsHitTestVisible = Cmb_BookOut.IsHitTestVisible = !(FormState.State == EnumFormStates.Edit);  //新增後不可編輯
        }

        //查詢收支欄位連動
        private void Qry_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(1, Qry_Action, Qry_ActionDtl, Qry_AcctCode, Qry_BookIn, Qry_BookOut);
        }

        //查詢交易明細欄位連動
        private void Qry_ActionDtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(2, Qry_Action, Qry_ActionDtl, Qry_AcctCode, Qry_BookIn, Qry_BookOut);
        }

        //查詢會計分類欄位連動
        private void Qry_AcctCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(3, Qry_Action, Qry_ActionDtl, Qry_AcctCode, Qry_BookIn, Qry_BookOut);
        }

        //收支欄位連動
        private void Cmb_Action_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(1, Cmb_Action, Cmb_ActionDtl, Cmb_AcctCode, Cmb_BookIn, Cmb_BookOut);
        }

        //交易明細欄位連動
        private void Cmb_ActionDtl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(2, Cmb_Action, Cmb_ActionDtl, Cmb_AcctCode, Cmb_BookIn, Cmb_BookOut);
        }

        //會計分類欄位連動
        private void Cmb_AcctCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CmbSyncUp_SelectionChanged(3, Cmb_Action, Cmb_ActionDtl, Cmb_AcctCode, Cmb_BookIn, Cmb_BookOut);
        }

        /// <summary>
        /// 欄位連動連動
        /// </summary>
        /// <param name="level">層級(1.Action、2.ActionDtl、3.AcctCode)</param>
        /// <param name="cb_Action">收支</param>
        /// <param name="cb_ActionDtl">交易明細</param>
        /// <param name="cb_AcctCode">會計分類</param>
        /// <param name="cb_BookIn">收入帳冊</param>
        /// <param name="cb_BookOut">支出帳冊</param>
        private void CmbSyncUp_SelectionChanged(int level, ComboBox cb_Action, ComboBox cb_ActionDtl, ComboBox cb_AcctCode, ComboBox cb_BookIn, ComboBox cb_BookOut)
        {
            string Act = (cb_Action.SelectedValue == null) ? "" : cb_Action.SelectedValue.ToString();
            string Dtl = (cb_ActionDtl.SelectedValue == null) ? "" : cb_ActionDtl.SelectedValue.ToString();
            string Code = (cb_AcctCode.SelectedValue == null) ? "" : cb_AcctCode.SelectedValue.ToString();
            string InOut = "";  //空白(全選)、In、Out、BankDeal

            //判斷交易類型
            if (level == 1 && Act == "")  //若收支切換成全選就清空
                InOut = "";
            else if (Act != "")
            {
                if (Act == "1")
                    InOut = "In";
                else if (Act == "2")
                    InOut = "Out";
                else if (Lst_BankDealTra.Contains(Act))
                    InOut = "BankDeal";
            }
            else if (Dtl != "")
            {
                if (Lst_TraDtlIn.Where(x => !string.IsNullOrWhiteSpace(x.item) && x.item == Dtl).Count() > 0)  //收入
                    InOut = "In";
                else if (Lst_TraDtlOut.Where(x => !string.IsNullOrWhiteSpace(x.item) && x.item == Dtl).Count() > 0)  //支出
                    InOut = "Out";
            }
            else if (Code != "")
            {
                if (Lst_AcctIn.Where(x => !string.IsNullOrWhiteSpace(x.item) && x.item == Code).Count() > 0)  //收入
                    InOut = "In";
                else if (Lst_AcctOut.Where(x => !string.IsNullOrWhiteSpace(x.item) && x.item == Code).Count() > 0)  //支出
                    InOut = "Out";
            }

            //異動相關欄位(依照層級決定要連動的欄位)
            if (InOut == "In")  //收入
            {
                if (level == 1)
                    cb_ActionDtl.ItemsSource = new List<MapFile>(Lst_TraDtlIn);
                if (level <= 2)
                    cb_AcctCode.ItemsSource = new List<MapFile>(Lst_AcctIn);

                if (Dtl == "a")  //入現金
                    cb_BookIn.ItemsSource = Lst_BookBase.Where(x => x.book_type == "1" || x.book == "").ToList();
                else if (Dtl == "b")  //入銀存
                    cb_BookIn.ItemsSource = Lst_BookBase.Where(x => x.book_type == "2" || x.book == "").ToList();
                else  //其他
                    cb_BookIn.ItemsSource = new List<BookBase>(Lst_BookBase);
                cb_BookOut.ItemsSource = Lst_BookBase.Where(x => x.book == "").ToList();  //清空
            }
            else if (InOut == "Out")  //支出
            {
                if (level == 1)
                    cb_ActionDtl.ItemsSource = new List<MapFile>(Lst_TraDtlOut);
                if (level <= 2)
                    cb_AcctCode.ItemsSource = new List<MapFile>(Lst_AcctOut);

                if (Dtl == "1")  //現金
                    cb_BookOut.ItemsSource = Lst_BookBase.Where(x => x.book_type == "1" || x.book == "").ToList();
                else if (Dtl == "2")  //銀行轉帳
                    cb_BookOut.ItemsSource = Lst_BookBase.Where(x => x.book_type == "2" || x.book == "").ToList();
                else  //其他
                    cb_BookOut.ItemsSource = new List<BookBase>(Lst_BookBase);
                cb_BookIn.ItemsSource = Lst_BookBase.Where(x => x.book == "").ToList();  //清空
            }
            else if (InOut == "BankDeal")  //調撥
            {
                cb_ActionDtl.ItemsSource = Lst_TraDtl.Where(x => x.item == "").ToList();
                cb_AcctCode.ItemsSource = Lst_Acct.Where(x => x.item == "").ToList(); ;
                cb_BookIn.ItemsSource = (new List<string> { "3", "5" }).Contains(Act) ? Lst_BookBase.Where(x => x.book_type == "2" || x.book == "").ToList() : Lst_BookBase.Where(x => x.book_type == "1" || x.book == "").ToList();
                cb_BookOut.ItemsSource = (new List<string> { "4", "5" }).Contains(Act) ? Lst_BookBase.Where(x => x.book_type == "2" || x.book == "").ToList() : Lst_BookBase.Where(x => x.book_type == "1" || x.book == "").ToList();
            }
            else  //全選
            {
                if (level == 1)
                    cb_ActionDtl.ItemsSource = new List<MapFile>(Lst_TraDtl);
                if (level <= 2)
                    cb_AcctCode.ItemsSource = new List<MapFile>(Lst_Acct);
                cb_BookIn.ItemsSource = new List<BookBase>(Lst_BookBase);
                cb_BookOut.ItemsSource = new List<BookBase>(Lst_BookBase);
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
                    Cmb_Action.SelectedValue = drv["action"];
                    Cmb_ActionDtl.SelectedValue = (drv["action_dtl"].ToString() == "0") ? "" : drv["action_dtl"];  //來源為0要轉空白
                    Cmb_AcctCode.SelectedValue = drv["acct_code"];
                    Cmb_BookIn.SelectedValue = drv["acct_book_in"];
                    Cmb_BookOut.SelectedValue = drv["acct_book_out"];
                    Txt_MemoDef.Text = drv["memodef"].ToString();
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
        private bool CheckBeforeEdit(EnumFormStates ChangeState)
        {
            if (ChangeState == EnumFormStates.Edit || ChangeState == EnumFormStates.Delete)
            {
                //if (!CommUtility.CheckIsPro((DateTime)Dtp_TradeDt.SelectedDate))
                //{
                //    MessageBox.Show($"{((DateTime)Dtp_TradeDt.SelectedDate).ToString("%M")}月已經結帳，" +
                //        $"不可{Enum.GetName(typeof(EnumFormStatesText), ChangeState)}", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                //    return false;
                //}
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState.State == EnumFormStates.Add)
            {
                string filter = "where 1=1 ";
                string q_action = (Cmb_Action.SelectedValue == null) ? "" : Cmb_Action.SelectedValue.ToString();
                filter += $"\r\n and action = '{q_action}'";

                string q_action_dtl = (Cmb_ActionDtl.SelectedValue == null) ? "" : Cmb_ActionDtl.SelectedValue.ToString();
                if (Lst_BankDealTra.Contains(q_action)) q_action_dtl = "0";  //調撥要放0
                filter += $"\r\n and action_dtl = '{q_action_dtl}'";

                string q_acct_code = (Cmb_AcctCode.SelectedValue == null) ? "" : Cmb_AcctCode.SelectedValue.ToString();
                filter += $"\r\n and acct_code = '{q_acct_code}'";

                string q_acct_book_in = (Cmb_BookIn.SelectedValue == null) ? "" : Cmb_BookIn.SelectedValue.ToString();
                filter += $"\r\n and acct_book_in = '{q_acct_book_in}'";

                string q_acct_book_out = (Cmb_BookOut.SelectedValue == null) ? "" : Cmb_BookOut.SelectedValue.ToString();
                filter += $"\r\n and acct_book_out = '{q_acct_book_out}'";

                if (q_action == "" && q_action_dtl == "" && q_acct_code == "" && q_acct_book_in == "" && q_acct_book_out == "")
                {
                    MessageBox.Show("「收支、交易方式、會計科目、收入帳冊、支出帳冊」不可皆為空", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                DataTable dt = DBService.QryTraMastMemoDef(filter);
                if (dt.Rows.Count > 0)
                {
                    MessageBox.Show("「收支、交易方式、會計科目、收入帳冊、支出帳冊」為Key值只能有一筆", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            rowView["action"] = Cmb_Action.SelectedValue == null ? "" : Cmb_Action.SelectedValue;
            rowView["action_dtl"] = Cmb_ActionDtl.SelectedValue == null ? "" : Cmb_ActionDtl.SelectedValue;
            rowView["acct_code"] = Cmb_AcctCode.SelectedValue == null ? "" : Cmb_AcctCode.SelectedValue;
            rowView["acct_book_in"] = Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue;
            rowView["acct_book_out"] = Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue;
            rowView["memodef"] = Txt_MemoDef.Text.Trim();
            rowView["loguser"] = AppVar.UserName;
            rowView["logtime"] = DateTime.Now;

            if (Lst_BankDealTra.Contains(rowView["action"].ToString()))
                rowView["action_dtl"] = "0";  //調撥要放0

            DBService.InsTraMastMemoDef(rowView);
            rowView.EndEdit();
        }

        //編輯作業
        private void Operation_Edit()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            rowView["action"] = Cmb_Action.SelectedValue == null ? "" : Cmb_Action.SelectedValue;
            rowView["action_dtl"] = Cmb_ActionDtl.SelectedValue == null ? "" : Cmb_ActionDtl.SelectedValue;
            rowView["acct_code"] = Cmb_AcctCode.SelectedValue == null ? "" : Cmb_AcctCode.SelectedValue;
            rowView["acct_book_in"] = Cmb_BookIn.SelectedValue == null ? "" : Cmb_BookIn.SelectedValue;
            rowView["acct_book_out"] = Cmb_BookOut.SelectedValue == null ? "" : Cmb_BookOut.SelectedValue;
            rowView["memodef"] = Txt_MemoDef.Text.Trim();
            rowView["loguser"] = AppVar.UserName;
            rowView["logtime"] = DateTime.Now;

            if (Lst_BankDealTra.Contains(rowView["action"].ToString()))
                rowView["action_dtl"] = "0";  //調撥要放0

            DBService.UpdTraMastMemoDef(rowView);
            rowView.EndEdit();
            drv.Sort = drv.Sort == "" ? "[action]" : "";  //透過改變Sort強迫觸發IValueConverter，但Sort只有空白時可以給值，所以並不會被賦予空白
        }

        private void Operation_Delete()
        {
            DataView drv = DG_Main.ItemsSource as DataView;
            int r = DG_Main.SelectedIndex;
            DataRowView rowView = drv[r];
            DBService.DelTraMastMemoDef(rowView);
            rowView.Delete();
            rowView.EndEdit();
        }
    }

    /// <summary>
    /// Gride文字轉換器(顯示用，只實作Convert)
    /// </summary>
    public class TraMastMemoDefGrideConverter : IValueConverter
    {
        List<MapFile> Lst_Tra = DBService.GetMapFile("1");
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
