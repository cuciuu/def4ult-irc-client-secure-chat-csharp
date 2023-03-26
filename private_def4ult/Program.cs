using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetIRC;
using NetIRC.Connection;
using NetIRC.Messages;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.Win32;
namespace private_def4ult
{
    public class Program
    {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main(string[] args)
        {

            if (File.Exists(@"C:\Program Files\OpenVPN\config\README.txt"))
            {

            }
            else {
                MessageBox.Show("Please install OVPN in order to join Def4ult Private Server", "OpenVPN not found!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                System.Diagnostics.Process.Start(@"bin\ovpn.exe");
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new def4ult());
        }

       

    }
}
