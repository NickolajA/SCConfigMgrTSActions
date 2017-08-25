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
    public partial class WebServiceControl: SmsOsdEditorPageControl
    {
        /// <summary>
        /// Default constructor (not used)
        /// </summary>
        public override void InitializePageControl()
        {
            base.InitializePageControl();
        }

        protected WebServiceControl()
        {
            this.Initialized = false;

            InitializeComponent();
        }

        /// <summary>
        /// Custom action constructor
        /// </summary>
        public WebServiceControl(SmsPageData data) : base(data)
        {
            this.Initialized = false;
            InitializeComponent();

            //' Set action name
            PropertyManager["Name"].StringValue = "Invoke Web Service Method";

            //' Initialize default values for controls in the property manager
            LoadDefaultPropertyValues();
            LoadControlsFromProperty();

            ControlsValidator.AddControl((Control)textBoxURL, new ControlDataStateEvaluator(ValidateURL), "Enter a valid URL for the web service. Should start with 'http://' or 'https://' and end with '.asmx'");
            ControlsValidator.AddControl((Control)textBoxMethod, new ControlDataStateEvaluator(ValidateMethod), "Empty method selection, validate the URL, load methods from web service and make a selection");
            ControlsValidator.AddControl((Control)textBoxParam, new ControlDataStateEvaluator(ValidateParams), "Invalid format input detected, supported input could be e.g. 'param1','param2' with the following special characters '_-.' allowed");
            ControlsValidator.ValidateAll();

            this.Initialized = true;
        }

        private ControlDataState ValidateURL()
        {
            if (textBoxURL.Text.StartsWith("http://") || textBoxURL.Text.StartsWith("https://"))
            {
                if (textBoxURL.Text.EndsWith(".asmx"))
                {
                    return ControlDataState.Valid;
                }
            }

            return ControlDataState.Invalid;
        }

        private ControlDataState ValidateMethod()
        {
            if (!String.IsNullOrEmpty(textBoxMethod.Text))
            {
                return ControlDataState.Valid;
            }

            return ControlDataState.Invalid;
        }

        private ControlDataState ValidateParams()
        {
            string pattern = @"^('[a-zA-Z0-9%-_. ]+')(\s*)(,'\s*[a-zA-Z0-9%-_. ]+')*$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (textBoxParam.Text.Length >= 2)
            {
                if (regex.IsMatch(textBoxParam.Text) == true)
                {
                    return ControlDataState.Valid;
                }
            }

            return ControlDataState.Invalid;
        }

        /// <summary>
        /// Test if a given url is accessible determined by status code
        /// </summary>
        public bool TestURLAccessible(string uri)
        {
            //' Variable for web response results
            bool results = false;

            //' Create new web request and with headers only
            WebRequest request = WebRequest.Create(uri);
            request.Timeout = 1000;
            HttpWebResponse response = null;

            try
            {
                //' Return true if response code OK
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    results = true;
                }
            }
            catch (WebException ex)
            {
                results = false;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return results;
        }

        /// <summary>
        /// Retrieve a list of available web methods for specified url
        /// Method modified from its original source: 
        /// http://mikehadlow.blogspot.se/2006/06/simple-wsdl-object.html
        /// </summary>
        private List<string> GetWebServiceMethods(Uri url)
        {
            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Query = "WSDL";

            List<string> methodNames = new List<string>();

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uriBuilder.Uri);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Method = "GET";
            webRequest.Accept = "text/xml";

            ServiceDescription serviceDescription;

            using (WebResponse response = webRequest.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                serviceDescription = ServiceDescription.Read(stream);
            }

            foreach (PortType portType in serviceDescription.PortTypes)
            {
                foreach (Operation operation in portType.Operations)
                {
                    //' Get the method name and add to list
                    methodNames.Add(operation.Name);
                }
            }

            return methodNames;
        }

        private void ControlsToProperty()
        {
            PropertyManager["Name"].StringValue = textBoxName.Text;
            PropertyManager["Description"].StringValue = textBoxDescription.Text;
            PropertyManager["WebServiceURL"].StringValue = textBoxURL.Text;
            PropertyManager["WebServiceMethodName"].StringValue = textBoxMethod.Text;
            PropertyManager["WebServiceParams"].StringValue = textBoxParam.Text;
        }

        private void LoadControlsFromProperty()
        {
            textBoxName.Text = PropertyManager["Name"].StringValue;
            textBoxDescription.Text = PropertyManager["Description"].StringValue;
            textBoxURL.Text = PropertyManager["WebServiceURL"].StringValue;
            textBoxMethod.Text = PropertyManager["WebServiceMethodName"].StringValue;
            textBoxParam.Text = PropertyManager["WebServiceParams"].StringValue;
        }

        private void LoadDefaultPropertyValues()
        {
            if (PropertyManager["Name"].ObjectValue == null)
            {
                PropertyManager["Name"].StringValue = "Invoke Web Service Method";
            }

            if (PropertyManager["Description"].ObjectValue == null)
            {
                PropertyManager["Description"].StringValue = string.Empty;
            }

            if (PropertyManager["WebServiceURL"].ObjectValue == null)
            {
                PropertyManager["WebServiceURL"].StringValue = string.Empty;
            }

            if (PropertyManager["WebServiceMethodName"].ObjectValue == null)
            {
                PropertyManager["WebServiceMethodName"].StringValue = string.Empty;
            }

            if (PropertyManager["WebServiceParams"].ObjectValue == null)
            {
                PropertyManager["WebServiceParams"].StringValue = string.Empty;
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

            if (textBoxParam.Text.Length >= 2)
            {
                textBoxParam.Text = textBoxParam.Text.Replace("\"", "'");
            }

            //' Push changes from the controls to PropertyManager
            ControlsToProperty();

            return base.ApplyChanges(out errorControl, out showError);
        }

        private void DirtyControl_TextChanged(object sender, EventArgs e)
        {
            //' Validate controls
            ControlsValidator.ValidateAll();

            this.SetDirty(true);
        }

        private void WebServiceControl_Load(object sender, EventArgs e)
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
                string.Equals(this.PropertyManager["WebServiceURL"].StringValue, this.textBoxURL.Text, StringComparison.OrdinalIgnoreCase) == false ||
                string.Equals(this.PropertyManager["WebServiceMethodName"].StringValue, this.textBoxMethod.Text, StringComparison.OrdinalIgnoreCase) == false ||
                string.Equals(this.PropertyManager["WebServiceParams"].StringValue, this.textBoxParam.Text, StringComparison.OrdinalIgnoreCase) == false)
            {
                dirty = true;
            }

            base.SetDirty(dirty);
        }

        private void ValidateURL_Click(object sender, EventArgs e)
        {
            bool result = false;

            if (!String.IsNullOrEmpty(textBoxURL.Text))
            {
                result = TestURLAccessible(textBoxURL.Text);
            }

            if (result == true)
            {
                buttonLoad.Enabled = true;
            }
            else
            {
                ShowMessageBox("URL validation failed", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                buttonLoad.Enabled = false;
            }
        }

        private void ButtonLoad_Click(object sender, EventArgs e)
        {
            //' Clear existing methods from control
            if (comboBoxMethod.Items.Count >= 1)
            {
                comboBoxMethod.Items.Clear();
            }

            //' Load web methods into control
            if (!String.IsNullOrEmpty(textBoxURL.Text))
            {
                Uri uri = new Uri(textBoxURL.Text);
                List<string> webMethods = GetWebServiceMethods(uri);
                webMethods.Sort();

                if (webMethods != null && webMethods.Count >= 1)
                {
                    foreach (string webMethod in webMethods)
                    {
                        comboBoxMethod.Items.Add(webMethod);
                    }
                    comboBoxMethod.SelectedIndex = 0;
                }
            }
        }

        private void ComboBoxMethod_SelectedValueChanged(object sender, EventArgs e)
        {
            textBoxMethod.Text = comboBoxMethod.SelectedItem.ToString();
        }
    }
}
