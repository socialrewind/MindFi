using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MBSite
{
    public partial class FBlogin_success : System.Web.UI.Page
    {
        const string FBBase = "https://www.facebook.com";

        protected void Page_Load(object sender, EventArgs e)
        {
            bool Success = false;

            if (this.Request.UrlReferrer != null)
            {
                string Referrer = this.Request.UrlReferrer.ToString();
                //this.lblAccessToken.Text = this.Request.UrlReferrer.ToString();
                if (Referrer.IndexOf(FBBase) == 0)
                {
                    Success = true;
                }
            }
            // interpret the Access token
            if (!Success)
            {
                this.lblResult.Text = "Login failed, this is not coming from Facebook";
                this.lblExpires.Text = this.Request.RawUrl;
            }
            else
            {
                this.lblResult.Text = "Login successful!";
                this.lblExpires.Text = this.Request.RawUrl;
            }
        }
    }
}