using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PostStatusLaterApp
{
    public partial class _Default : System.Web.UI.Page
    {
        string error;
        string error_reason;
        string error_description;
        string code;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                error = Request.Params["error"];
                error_reason = Request.Params["error_reason"];
                error_description = Request.Params["error_description"];
                code = Request.Params["code"];

                if (error == null || error == "")
                {
                    lblError.Text = "Code received: " + code;
                }
                else
                {
                    lblError.Text = "Error from Facebook authorization: " + error + "<br>" + error_reason + "<br>" + error_description;
                }
                InitializeControls();
            }
        }

        private void InitializeControls()
        {
            lstHour.Items.Clear();
            for (int i = 0; i < 24; i++)
            {
                string hourToShow = i.ToString();
                if (hourToShow.Length < 2)
                {
                    hourToShow = "0" + hourToShow;
                }
                lstHour.Items.Add(hourToShow);
            }
            lstMinute.Items.Clear();
            for (int i = 0; i < 60; i++)
            {
                string minuteToShow = i.ToString();
                if (minuteToShow.Length < 2)
                {
                    minuteToShow = "0" + minuteToShow;
                }
                lstMinute.Items.Add(minuteToShow);
            }
        }

        protected void Calendar1_SelectionChanged(object sender, EventArgs e)
        {

        }
    }
}
