using Accounting_App.DTO;
using Accounting_App.Utilities;
using System;
using System.Collections.Generic;
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

namespace Accounting_App.Form
{
    /// <summary>
    /// Form_bas_user.xaml 的互動邏輯
    /// </summary>
    public partial class Form_bas_user : Window
    {
        FormState FormState = new FormState();

        public Form_bas_user()
        {
            InitializeComponent();
            FormInitial();
        }

        private void FormInitial()
        {
            DG_Main.ItemsSource = DBService.QryBasUser();
            BtnGroup_CRUD.permission = DBService.GetBasRolePermission(AppVar.User.role_id, AppVar.OpenMenuId);
            BtnGroup_CRUD.Btn_Add.Click += (s, e) => { Btn_AED_Click(FormStateS.Add); };
            BtnGroup_CRUD.Btn_Edit.Click += (s, e) => { Btn_AED_Click(FormStateS.Edit); };
            BtnGroup_CRUD.Btn_Delete.Click += (s, e) => { Btn_AED_Click(FormStateS.Delete); };
            BtnGroup_CRUD.Btn_Save.Click += Btn_Save_Click;
            BtnGroup_CRUD.Btn_Cancel.Click += (s, e) => { Refresh(FormStateS.ShowData); };
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
                MessageBox.Show($"{FormState.StateText}失敗，錯誤訊息：" + ex.Message.ToString(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            //先觸發改變選單，再觸發SelectionChanged
            if (FormState.State == FormStateS.Initial || FormState.State == FormStateS.ShowData)
            {
                Cmb_RoleId.ItemsSource = DBService.QryBasRole();
                Cmb_DeptId.ItemsSource = DBService.QryBasDept();
                DG_Main_SelectionChanged(DG_Main, null);
            }
            else if (FormState.State == FormStateS.Add)
            {
                Cmb_RoleId.SelectedIndex = Cmb_DeptId.SelectedIndex = 0;
                Txt_UserId.Text = Txt_Name.Text = Txt_PassWord.Password = "";
                Chk_Enabled.IsChecked = true;
            }
            Txt_UserId.IsHitTestVisible = !(FormState.State == FormStateS.Edit);  //新增後不可編輯
        }

        //物件與Grid連動
        private void DG_Main_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DG_Main.SelectedItems.Count > 0)
            {
                foreach (var r in DG_Main.SelectedItems)
                {
                    BasUser drv = r as BasUser;
                    Txt_UserId.Text = drv.user_id;
                    Txt_Name.Text = drv.name;
                    Txt_PassWord.Password = drv.password;
                    Cmb_RoleId.SelectedValue = drv.role_id;
                    Cmb_DeptId.SelectedValue = drv.dept_id;
                    Chk_Enabled.IsChecked = drv.enabled;
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
            if (ChangeState == FormStateS.Delete)
            {
                MessageBox.Show("注意：帳號還未使用的情況才可刪除，一般情況請使用「停用」!", "刪除警示", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return true;
        }

        //儲存檢核
        private bool CheckBefoeSave()
        {
            if (FormState.State == FormStateS.Add)
            {
                var IsUserIdExists = DBService.QryBasUser($@"where user_id = '{Txt_UserId.Text}'");
                if (IsUserIdExists.Count > 0)
                {
                    MessageBox.Show("帳號已存在", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            if (FormState.State == FormStateS.Add || FormState.State == FormStateS.Edit)
            {
                if (Txt_UserId.Text == "" || Txt_Name.Text == "" || Txt_PassWord.Password == "")
                {
                    MessageBox.Show("帳號、姓名、密碼必填", "檢核失敗", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        //新增作業
        private void Operation_Add()
        {
            BasUser rowView = new BasUser()
            {
                user_id = Txt_UserId.Text.Trim(),
                name = Txt_Name.Text.Trim(),
                password = Txt_PassWord.Password.Trim(),
                role_id = Convert.ToString(Cmb_RoleId.SelectedValue),
                dept_id = Convert.ToString(Cmb_DeptId.SelectedValue),
                enabled = Chk_Enabled.IsChecked ?? false,
                loguser = AppVar.User.user_id,
                logtime = DateTime.Now
            };

            rowView.InsertDB();
            (DG_Main.ItemsSource as List<BasUser>).Add(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        //編輯作業
        private void Operation_Edit()
        {
            var rowView = DG_Main.SelectedItem as BasUser;
            rowView.name = Txt_Name.Text.Trim();
            rowView.password = Txt_PassWord.Password.Trim();
            rowView.role_id = Convert.ToString(Cmb_RoleId.SelectedValue);
            rowView.dept_id = Convert.ToString(Cmb_DeptId.SelectedValue);
            rowView.enabled = Chk_Enabled.IsChecked ?? false;
            rowView.loguser = AppVar.User.user_id;
            rowView.logtime = DateTime.Now;

            rowView.UpdateDB();
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }

        private void Operation_Delete()
        {
            var rowView = DG_Main.SelectedItem as BasUser;
            rowView.DeleteDB();
            (DG_Main.ItemsSource as List<BasUser>).Remove(rowView);
            CollectionViewSource.GetDefaultView(DG_Main.ItemsSource).Refresh();
        }
    }
}
