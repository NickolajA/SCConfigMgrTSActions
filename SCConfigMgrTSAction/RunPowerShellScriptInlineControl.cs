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
using System.Web.Services.Description;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace SCConfigMgrTSAction
{
    public partial class RunPowerShellScriptInlineControl : SmsOsdEditorPageControl
    {
        /// <summary>
        /// Default constructor (not used)
        /// </summary>
        public override void InitializePageControl()
        {
            base.InitializePageControl();
        }

        protected RunPowerShellScriptInlineControl()
        {
            this.Initialized = false;

            InitializeComponent();
        }

        /// <summary>
        /// Custom action constructor
        /// </summary>
        public RunPowerShellScriptInlineControl(SmsPageData data) : base(data)
        {
            this.Initialized = false;
            InitializeComponent();

            //' Initialize default values for controls in the property manager
            LoadDefaultPropertyValues();

            //' Load existing values from property manager
            LoadControlsFromProperty();


            this.Initialized = true;
        }

        private void SetPropertyFromControls()
        {
            PropertyManager["Name"].StringValue = textBoxName.Text;
            PropertyManager["Description"].StringValue = textBoxDescription.Text;
            PropertyManager["ScriptBlock"].StringValue = textBoxScript.Text;
        }

        private void LoadControlsFromProperty()
        {
            textBoxName.Text = PropertyManager["Name"].StringValue;
            textBoxDescription.Text = PropertyManager["Description"].StringValue;
            textBoxScript.Text = PropertyManager["ScriptBlock"].StringValue;
        }

        private void LoadDefaultPropertyValues()
        {
            if (PropertyManager["Name"].ObjectValue == null)
            {
                PropertyManager["Name"].StringValue = "Run PowerShell Script Inline";
            }

            if (PropertyManager["Description"].ObjectValue == null)
            {
                PropertyManager["Description"].StringValue = string.Empty;
            }

            if (PropertyManager["ScriptBlock"].ObjectValue == null)
            {
                PropertyManager["ScriptBlock"].StringValue = string.Empty;
            }
        }

        protected override bool ApplyChanges(out Control errorControl, out bool showError)
        {
            //' Check for error and return false
            if (HasError(out errorControl) == true)
            {
                ShowMessageBox(GetErrorString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                errorControl = null;
                showError = true;

                return false;
            }

            //' Push changes from the controls to PropertyManager
            SetPropertyFromControls();

            return base.ApplyChanges(out errorControl, out showError);
        }

        private void DirtyControl_TextChanged(object sender, EventArgs e)
        {
            //' Validate controls
            //ControlsValidator.ValidateAll();

            this.SetDirty(true);
        }

        private void RunPowerShellScriptInlineControl_Load(object sender, EventArgs e)
        {
            //' Load existing values for controls
            LoadControlsFromProperty();

            if (this.Initialized == false)
            {
                return;
            }

            bool dirty = false;

            //' Check if control values needs to be updated
            if (string.Equals(this.PropertyManager["Name"].StringValue, this.textBoxName.Text, StringComparison.OrdinalIgnoreCase) == false ||
                string.Equals(this.PropertyManager["Description"].StringValue, this.textBoxDescription.Text, StringComparison.OrdinalIgnoreCase) == false ||
                string.Equals(this.PropertyManager["ScriptBlock"].StringValue, this.textBoxScript.Text, StringComparison.OrdinalIgnoreCase) == false)
            {
                dirty = true;
            }

            base.SetDirty(dirty);
        }
    }
}
