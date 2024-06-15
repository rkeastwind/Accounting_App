using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting_App.Utilities
{
    public enum FormStateS
    {
        [Description("初始")]
        Initial,
        [Description("顯示")]
        ShowData,
        [Description("新增")]
        Add,
        [Description("修改")]
        Edit,
        [Description("刪除")]
        Delete
    };

    public class FormState
    {
        private FormStateS _st;
        public FormState()
        {
            _st = FormStateS.Initial;
        }

        public FormStateS State
        {
            get { return _st; }
            set { _st = value; }
        }

        public string StateText
        {
            get { return _st.GetDescriptionText(); }
        }
    }
}
