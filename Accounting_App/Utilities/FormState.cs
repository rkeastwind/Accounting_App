using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.Utilities
{
    public enum EnumFormStates { Initial, ShowData, Add, Edit, Delete };
    public enum EnumFormStatesText { 初始, 顯示, 新增, 修改, 刪除 };

    public class FormState
    {
        private EnumFormStates _st;
        public FormState()
        {
            this._st = EnumFormStates.Initial;
        }

        public EnumFormStates State
        {
            get { return this._st; }
            set { this._st = value; }
        }

        public string StateText
        {
            get { return Enum.GetName(typeof(EnumFormStatesText), _st); }
        }
    }
}
