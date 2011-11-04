using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MBBetaAPI.AgentAPI
{
    // TODO: Refactor, particularly locking and generalizing for Twitter, LinkedIn etc.
    // TODO: Progressive permissions according to the backup type

    public class FBLogin
    {
        private static volatile Object obj = new Object();
        private const string APPID = "131706850230259";
        private const string APPKey = "89c861c469cff95970836f3b8021d7bd";
        private const string AuthURL = "https://www.facebook.com/dialog/oauth?client_id=";
        //private const string RedirURL = "http://www.sinergia.net.mx/fb/login_success.html";
        private const string RedirURL = "http://www.sinergia.net.mx/socialrewind/fblogin_success.aspx";
        private const string ErrorURL = "http://www.sinergia.net.mx/fb/login_failure.html";
        private const string Permissions = "user_about_me,friends_about_me,user_birthday,friends_birthday,user_education_history,friends_education_history,user_events,friends_events,user_groups,friends_groups,user_hometown,friends_hometown,user_interests,friends_interests,user_likes,friends_likes,user_location,friends_location,user_notes,friends_notes,user_photos,friends_photos,user_photo_video_tags,friends_photo_video_tags,user_relationships,friends_relationships,user_relationship_details,friends_relationship_details,user_religion_politics,friends_religion_politics,user_status,friends_status,user_website,friends_website,user_work_history,friends_work_history,email,read_friendlists,read_mailbox,read_stream"; // ,user_address,user_mobile_phone";

        private static volatile bool m_loginStatus = false;
        private static volatile string m_accessToken = "";
        private static volatile string m_loginname;
        private static volatile FBPerson m_me;
        private static volatile string lastError;
        private static volatile int expires;

        public static string LoginName
        {
            get { return m_loginname; }
            set { lock (obj) { m_loginname = value; } }
        }

        public static void Login(out string URL, out CallBack myCallBack)
        {
            // get OAUTH page
            myCallBack = new CallBack(FBLogin.callbackLoginResult);

            // TODO: state parameter to prevent CSRF https://developers.facebook.com/docs/authentication/
            URL = AuthURL + APPID + "&redirect_uri=" + RedirURL + "&scope=" + Permissions + "&response_type=token&popup";
        }

        public static bool CheckCallback(string URL)
        {
            int start = URL.IndexOf(RedirURL) ;
            if (start == 0)
            {
                // Client code #access_token=...
                const string SaccessToken = "#access_token=";
                m_accessToken = URL.Substring(start + FBLogin.RedirURL.Length + SaccessToken.Length);
                const string expiresToken = "&expires_in=";

                int expiresIndex = m_accessToken.IndexOf(expiresToken);
                if (expiresIndex >= 0)
                {
                    if (!int.TryParse(m_accessToken.Substring(expiresIndex + expiresToken.Length), out expires))
                    {
                        expires = 0; // default
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
            AsyncReqQueue apiReq = FBAPI.Me(GetMe);
            apiReq.Queue();
            apiReq.Send();
            return loggedIn;
        }

        public static void LogOut()
        {
            lock (obj)
            {
                m_me = null;
                m_loginStatus = false;
                m_accessToken = "";
            }
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
