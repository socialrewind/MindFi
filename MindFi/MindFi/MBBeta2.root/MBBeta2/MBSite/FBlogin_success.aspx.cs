using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MBSite
{
    public partial class FBlogin_success : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            bool Success = true; // false;
            string userSNID = Request.QueryString["userSNID"];
            if (!Success)
            {
                this.lblResult.Text = "Login failed";
            }
            else
            {
                this.lblResult.Text = "Login successful! ";
                if (userSNID != null)
                {
                    this.lblResult.Text += userSNID;
                }
            }
        }
    }
}