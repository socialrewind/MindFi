using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace DBUtil
{
    /// <summary>
    /// Class that contains the main agent test program
    /// </summary>

    public static class DBUtilMain
    {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
	public static void Main()
	{

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // For new agent UI
            Application.Run(new DBUtil());
	}

    }
}
