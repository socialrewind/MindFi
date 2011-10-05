using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace MyBackup
{
    /// <summary>
    /// Class that contains the main agent test program
    /// </summary>

    public static class AgentTest
    {
	// private static FBLogin login;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
	public static void Main()
	{

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // For new agent UI
            Application.Run(new frmDashboard());
	    // for previous model
//            Application.Run(new frmMain());
            // for testing, particularly parsers
            //Application.Run(new frmTest());
	}

    }
}
