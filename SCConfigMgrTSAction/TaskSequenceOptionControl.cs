using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.ConfigurationManagement.AdminConsole;
using Microsoft.ConfigurationManagement.AdminConsole.TaskSequenceEditor;

namespace SCConfigMgrTSAction
{
    public class WebServiceOptionControl : TaskSequenceOptionControl
    {
        public WebServiceOptionControl(SmsPageData data) : base(data)
        {
            //' Set title for Option page from Resources
            this.Title = Properties.Resources.ActionOptionPageTitle;
        }
    }

    public class PromptTSVariableOptionControl : TaskSequenceOptionControl
    {
        public PromptTSVariableOptionControl(SmsPageData data) : base(data)
        {
            //' Set title for Option page from Resources
            this.Title = Properties.Resources.ActionOptionPageTitle;
        }
    }
}
