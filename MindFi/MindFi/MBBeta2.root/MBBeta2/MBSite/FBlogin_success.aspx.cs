using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

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
                    //this.lblResult.Text += userSNID;
                    // TODO: Save
                    try
                    {
                        System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/socialrewind");
                        System.Configuration.ConnectionStringSettings connString;
                        if (0 < rootWebConfig.ConnectionStrings.ConnectionStrings.Count)
                        {
                            connString = rootWebConfig.ConnectionStrings.ConnectionStrings["SocialRewind"];

                            using (SqlConnection conn = new SqlConnection(connString.ConnectionString))
                            {
                                conn.Open();
                                SqlCommand insCmd = new SqlCommand("insert into Logins (SNID) values (@SNID)", conn);
                                insCmd.Parameters.Add(new SqlParameter("SNID", userSNID));
                                insCmd.ExecuteNonQuery();
                                conn.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // ignore for now
                        Response.Write(ex.ToString());
                    }
                }
            }
        }
    }
}