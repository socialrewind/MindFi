using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;

namespace MBBetaAPI.AgentAPI
{
    // TODO: Refactor, particularly locking and generalizing for Twitter, LinkedIn etc.
    // TODO: Progressive permissions according to the backup type

    public class FBLogin
    {
        private static volatile Object obj = new Object();
        private const string APPID = "131706850230259";
        private const string AuthURL = "https://www.facebook.com/dialog/oauth?client_id=";
        private const string RedirURL = "http://www.socialrewind.com/socialrewind/FBlogin_success.aspx";
        private const string ErrorURL = "http://www.socialrewind.com/socialrewind/login_failure.html";
        private const string LogoutURL = "http://www.socialrewind.com/socialrewind/FBLogout.aspx";
        private const string Permissions = "user_about_me,friends_about_me,user_birthday,friends_birthday,user_education_history,friends_education_history,user_events,friends_events,user_groups,friends_groups,user_hometown,friends_hometown,user_interests,friends_interests,user_likes,friends_likes,user_location,friends_location,user_notes,friends_notes,user_photos,friends_photos,user_photo_video_tags,friends_photo_video_tags,user_relationships,friends_relationships,user_relationship_details,friends_relationship_details,user_religion_politics,friends_religion_politics,user_status,friends_status,user_website,friends_website,user_work_history,friends_work_history,email,read_friendlists,read_mailbox,read_stream,publish_stream"; // ,user_address,user_mobile_phone";

        private static volatile bool m_loginStatus = false;
        private static volatile string m_accessToken = "";
        private static volatile string m_loginname = "";
        private static volatile string m_loginuser = "0";
        private static volatile FBPerson m_me;
        private static volatile string lastError;
        private static volatile int expires;

        public static string LoginName
        {
            get { return m_loginname; }
            set { lock (obj) { m_loginname = value; } }
        }

        public static string LoginUser
        {
            get { return m_loginuser; }
            set { lock (obj) { m_loginuser = value; } }
        }

        public static void Login(out string URL, out CallBack myCallBack)
        {
            // get OAUTH page
            myCallBack = new CallBack(FBLogin.callbackLoginResult);

            // TODO: state parameter to prevent CSRF https://developers.facebook.com/docs/authentication/
            URL = AuthURL + APPID + "&redirect_uri=" + RedirURL 
                + "%3fuserSNID%3d" + m_loginuser
                + "&scope=" + Permissions + "&response_type=token&popup";
        }

        public static bool CheckCallback(string URL)
        {
            int start = URL.IndexOf(RedirURL) ;
            if (start == 0)
            {
                // Client code #access_token=...
                const string SaccessToken = "#access_token=";
                int startToken = URL.IndexOf(SaccessToken);
                //m_accessToken = URL.Substring(start + FBLogin.RedirURL.Length + SaccessToken.Length);
                m_accessToken = URL.Substring(startToken + SaccessToken.Length);
                const string expiresToken = "&expires_in=";

                int expiresIndex = m_accessToken.IndexOf(expiresToken);
                if (expiresIndex >= 0)
                {
                    int temp;

                    if (!int.TryParse(m_accessToken.Substring(expiresIndex + expiresToken.Length), out temp))
                    {
                        expires = 0; // default
                    }
                    else
                    {
                        expires = temp;
                    }
                    m_accessToken = m_accessToken.Substring(0, expiresIndex);
                }
                if (m_accessToken != "")
                {
                    return true;
                }
            }
            return false;
        }

        public static bool CheckErrorPage(string URL)
        {
            int start = URL.IndexOf(ErrorURL);
            if (start == 0)
            {
                return true;
            }
            return false;
        }

        public static bool callbackLoginResult(int hwnd, bool result, string response, Int64? parent, string parentSNID)
        {
            lock (obj)
            {
                m_loginStatus = result;
                m_accessToken = response;
            }
            if (result)
            {
                AsyncReqQueue apiReq = FBAPI.Me(GetMe);
                apiReq.Queue();
                apiReq.Send();
            }
            return loggedIn;
        }

        public static void LogOut(out string URL)
        {
            lock (obj)
            {
                m_me = null;
                m_loginStatus = false;
                m_accessToken = "";
            }
            URL = LogoutURL;
        }

        public static string token
        {
            get
            {
                return m_accessToken;
            }
        }

        public static bool loggedIn
        {
            get
            {
                return m_loginStatus;
            }
        }

        public static string LastError
        {
            get
            {
                return lastError;
            }
        }

        public static FBPerson Me
        {
            get
            {
                lock (obj)
                {
                    return m_me;
                }
            }
        }

        public static bool GetMe(int hwnd, bool result, string response, Int64? parent, string parentSNID)
        {
            lock (obj)
            {
                if (result)
                {
                    // distance to Me: 0
                    //string error;
                    m_me = new FBPerson(response, 0, null);
                    m_me.Parse();
                    LoginName = m_me.Name;
                    LoginUser = m_me.SNID;
                    lastError = response + "\n" + m_me.lastError;
                    string errorData = "";
                    m_me.Save(out errorData);
                    lastError += "\n" + errorData;

                }
                else
                {
                    lastError = "ERROR: " + response;
                    return false;
                }
                return true;
            }
        }


    }
}
