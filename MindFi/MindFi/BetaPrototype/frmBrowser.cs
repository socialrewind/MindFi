using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Form that navigates to FB Login page and helps in Logout too
    /// </summary>
    public partial class frmBrowser : Form
    {
        #region "Internal variables"
        //const string successURL = "https://www.facebook.com/connect/login_success.html";
        const string successURL = "http://www.sinergia.net.mx/fb/login_success.html";
        const string logoutURL = "https://www.facebook.com/logout.php";
        bool success = false;
        string accessCode = "";
        CallBack functionToCall;
        int expires;
        #endregion

        #region "Methods"
        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="URL">Login page</param>
        /// <param name="resultCall">Callback that responds once the browser is ready</param>
        public frmBrowser(string URL, CallBack resultCall)
        {
            InitializeComponent();
            functionToCall = resultCall;
            loadInBrowser(URL);
        }

	public void Logout()
	{
	    this.Show();
	    webBrowser1.Navigate(new Uri("https://www.facebook.com/logout.php"));
	}

        private void loadInBrowser(string URL)
        {
            this.webBrowser1.Navigate(URL);
        }

        #endregion

        #region "Internal functions"

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            string checkLogout = webBrowser1.Document.Url.ToString();
            if (checkLogout.IndexOf(successURL) < 0)
            {
		// MessageBox.Show("not login success\n" + checkLogout);
	    try {
                webBrowser1.Document.InvokeScript("document.forms['logout_form'].submit();");
	    } catch ( Exception ex )
	    {
		MessageBox.Show("error invoking logout: " + ex.ToString() );
	    }

                timer1.Interval = 50000;
                timer1.Enabled = true;
                return;
            }

            // Check redirect
            string Check = webBrowser1.Url.ToString();
            //int start = Check.IndexOf(successURL) ;
            int start = Check.IndexOf(FBLogin.RedirURL) ;
            if (start == 0)
            {
                // parse the code
                // Server code code=...
                // accessCode = Check.Substring(start + successURL.Length + 6);
                // Client code #access_token=...
                const string SaccessToken = "#access_token=";

                //accessCode = Check.Substring( start + successURL.Length + SaccessToken.Length );
                accessCode = Check.Substring( start + FBLogin.RedirURL.Length + SaccessToken.Length );
                const string expiresToken = "&expires_in=";
                
                int expiresIndex = accessCode.IndexOf(expiresToken);
                if (expiresIndex >= 0)
                {
                    if (!int.TryParse(accessCode.Substring(expiresIndex + expiresToken.Length), out expires))
                    {
                        expires = 0; // default
                    }
                    accessCode = accessCode.Substring(0, expiresIndex);
                }
                success = true;
                timer1.Interval = 500;
                timer1.Enabled = true;
            }
            else
            {
                // show when there is an error for allowing manual login
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
            }
            
        }

        private void frmBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!success)
            {
                accessCode = "";                
            }
            // callback?
            if (functionToCall != null)
            {
                functionToCall(this.Handle.ToInt32(), success, accessCode, null, "");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // close after a few seconds
            this.Close();
        }

private void webBrowser1_Navigating(object sender, 
    WebBrowserNavigatingEventArgs e)
{
// MessageBox.Show("Navigating to " + e.Url.ToString());
}


        #endregion

    }
}
