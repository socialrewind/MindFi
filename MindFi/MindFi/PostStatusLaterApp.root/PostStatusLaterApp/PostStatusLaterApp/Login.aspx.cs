using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PostStatusLaterApp
{
    public partial class Login : System.Web.UI.Page
    {
        const string AuthURL = "https://www.facebook.com/dialog/oauth";
        const string APPID = "312465115439932";
        const string RedirURL = "http://www.socialrewind.com/socialrewind/psl/default.aspx";
        const string Scope = "publish_stream";

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect(AuthURL + "?client_id=" + APPID
                + "&redirect_uri=" + Server.UrlEncode(RedirURL)
                + "&scope=" + Scope);
        }
    }
}