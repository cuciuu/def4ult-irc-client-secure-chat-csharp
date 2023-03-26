using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetIRC;
using NetIRC.Connection;
using NetIRC.Messages;

namespace private_def4ult
{
    public partial class def4ult : Form
    {
        // Declare a private variable to store the nickname
        private string nickname = "";

        // Declare a constant string to represent the IRC channel
        private const string channel = "#def4ult";

        // Declare a static string to store the key hash
        public static string hash = "def4ult";

        // Declare a static instance of the IRC client
        private static Client client;

        // Constructor method for the form
        public def4ult()
        {
            InitializeComponent();
        }

        // Method to encrypt a string using TripleDES algorithm
        private string encrypt(string stringkey)
        {
            // Convert the input string to a byte array
            byte[] data = UTF8Encoding.UTF8.GetBytes(stringkey);

            // Create a new MD5 hash object
            using (MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider())
            {
                // Compute the hash of the key string and store it in a byte array
                byte[] keys = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(hash));

                // Create a new TripleDES object and set its key, mode, and padding mode
                using (TripleDESCryptoServiceProvider tripDes = new TripleDESCryptoServiceProvider() { Key = keys, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 })
                {
                    // Create a new encryption transform object
                    ICryptoTransform transform = tripDes.CreateEncryptor();

                    // Use the transform object to encrypt the input data
                    byte[] results = transform.TransformFinalBlock(data, 0, data.Length);

                    // Convert the encrypted data to a base64-encoded string and return it
                    stringkey = Convert.ToBase64String(results, 0, results.Length);
                    return stringkey;
                }
            }
        }
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        private int xclose = 1;
        static bool ConsoleEventCallback(int eventType)

        {
            if (eventType == 2)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/f /im openvpn.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }).WaitForExit();
            }
            return false;
        }
        static ConsoleEventDelegate handler;   // Keeps it from getting garbage collected
                                               // Pinvoke
        private delegate bool ConsoleEventDelegate(int eventType);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetConsoleCtrlHandler(ConsoleEventDelegate callback, bool add);
        
        // Load event handler for form
        private void def4ult_Load(object sender, EventArgs e)
        {
            // Set ConsoleEventDelegate
            handler = new ConsoleEventDelegate(ConsoleEventCallback);

            // Set ConsoleCtrlHandler
            SetConsoleCtrlHandler(handler, true);

            // Disconnect openvpn
            disconnect("openvpn.exe");

            // Start cmd process to install openvpn silently
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "openvpn-install-2.4.9-I601-Win10.exe /S";
            process.StartInfo = startInfo;
            process.Start();

            // Set label text
            label_main.Text = "def4ult irc private server : " + System.DateTime.Now.DayOfWeek.ToString();

            // Hide encrypt panel, chat button, and lines
            encrypt_panel.Visible = false;
            line2.Visible = false;
            chatbtn.Visible = false;
            line3.Visible = false;

            // Get handle to console window
            var handle = GetConsoleWindow();

            // Hide console window
            ShowWindow(handle, SW_HIDE);
        }


       

        private void encryptkey_Click(object sender, EventArgs e)
        {
            string_key.Text = encrypt(string_key.Text);
            Clipboard.SetText(string_key.Text);

        }

        private void encrypt_btn_Click(object sender, EventArgs e)
        {
            encrypt_panel.Visible = true;
            login_page.Visible = false;
            line2.Visible = true; line1.Visible = false;
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            encrypt_panel.Visible = false;
            login_page.Visible = true;
            line2.Visible = false; line1.Visible = true;
        }
        private void enter_btn_Click(object sender, EventArgs e)
        {
            // Check if the username is empty, default or null. If so, show an error message
            if (usernametxt.Text == "" || usernametxt.Text == "username" || usernametxt.Text == "def4ult" || usernametxt.Text == null)
            {
                MessageBox.Show("Username required.", "Key System : def4ult", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                // Check if the key is correct by comparing it to the encryption of the current day of the week
                if (keytxt.Text == encrypt(System.DateTime.Now.DayOfWeek.ToString()))
                {
                    // Generate a random number and append it to the username to create a nickname
                    Random rnd = new Random();
                    int rndnr = rnd.Next(999);
                    nickname = usernametxt.Text + "_" + rndnr;
                    // Show the chat button
                    chatbtn.Visible = true;
                }
                else
                {
                    // Show an error message if the key is incorrect
                    MessageBox.Show("Incorrect Key.", "Key System : def4ult", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private async void Client_RawDataReceived(Client client, string rawData)
        {
            WriteLine(rawData);

        }

        private async void EventHub_RegistrationCompleted(object sender, EventArgs e)
        {
            await client.SendAsync(new JoinMessage(channel));
            // Here a message that gets truncated

        }

        // Just a simple Console.WriteLine wrapper to allow us to change font color
        public async void WriteLine(string value, ConsoleColor color = ConsoleColor.White)
        {
            // Cache previous console foreground color to restore it later
            var previousColor = Console.ForegroundColor;

            try
            {
                // Extract the relevant message information
                string pharse2 = value;
                string phrase = value;
                phrase = phrase.Split(' ').Last();
                phrase = phrase.Substring(1);
                string[] words = value.Split(' ');
                string lastmessage = words[3].ToString();
                value = value.Replace(lastmessage, "");

                // Remove any characters after the '?' character
                int index = value.LastIndexOf("!");
                if (index > 0)
                {
                    value = value.Substring(0, index);
                }

                // Construct the final message to display in console
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string final_msg = "def4ult team - user: " + value.Substring(1) + " : " + decrypt(phrase);

                // Check if the final message contains an error message
                if (final_msg.Contains("Incorrect Format"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n" + pharse2);
                    Console.WriteLine(final_msg + " (error: he is not using the custom client)\n");
                }
                else
                {
                    // Display the final message
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("\n" + final_msg + "\n");
                }

                // Check if the final message contains a specific error message
                if (final_msg.Contains(" :End of /NAMES list. : Incorrect Format"))
                {
                    // Display a warning message and the user's name
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("================================================");
                    Console.WriteLine("|          Def4ult Private IRC Server          |");
                    Console.WriteLine("|                                              |");
                    Console.WriteLine("|    * *  *      W A R N I N G     *  * *      |");
                    Console.WriteLine("|                                              |");
                    Console.WriteLine("|         This is a private irc server         |");
                    Console.WriteLine("|                                              |");
                    Console.WriteLine("| If you're not a member of def4ult leave ASAP |");
                    Console.WriteLine("| Users of this system have no expectation of  |");
                    Console.WriteLine("| privacy. By continuing, you consent to your  |");
                    Console.WriteLine("| keystrokes and data content being monitored  |");
                    Console.WriteLine("|                                              |");
                    Console.WriteLine("================================================");
                    Console.WriteLine("|                SEND A MESSAGE                |");
                    Console.WriteLine("================================================");
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("USER: " + nickname + "");
                }
            }
            catch
            {
                // If an exception occurs, just print the value
                Console.WriteLine(value);
            }

            // Restore the previous console foreground color
            Console.ForegroundColor = previousColor;

            // Set the data to the given value
            data = value;
        }

        public static string data;


        public string decrypt(string unprt)
        {
            try
            {
                using (TripleDESCryptoServiceProvider tripleDESCryptoService = new TripleDESCryptoServiceProvider())
                {
                    using (MD5CryptoServiceProvider hashMD5Provider = new MD5CryptoServiceProvider())
                    {
                        byte[] byteHash = hashMD5Provider.ComputeHash(Encoding.UTF8.GetBytes(hash));
                        tripleDESCryptoService.Key = byteHash;
                        tripleDESCryptoService.Mode = CipherMode.ECB;
                        byte[] data = Convert.FromBase64String(unprt);
                        unprt = Encoding.UTF8.GetString(tripleDESCryptoService.CreateDecryptor().TransformFinalBlock(data, 0, data.Length));

                        return unprt;
                    }
                }
            }
            catch { return "Incorrect Format"; }
        }

        private async void chatbtn_Click(object sender, EventArgs e)
        {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            connect();

            var handle = GetConsoleWindow();


            // Show
            ShowWindow(handle, SW_SHOW);
            this.Visible = false;
            line3.Visible = true;
            var user = new User(nickname, "def4ult");

            // irc ip
            var tcpConnection = new TcpClientConnection("164.92.229.224", 6667);
            using (client = new Client(user, tcpConnection))
            {
                client.RawDataReceived += Client_RawDataReceived;
                client.RegistrationCompleted += EventHub_RegistrationCompleted;
                client.RegisterCustomMessageHandlers(typeof(Program).Assembly);

                Console.WriteLine("================================================");
                Console.WriteLine("|          Def4ult Private IRC Server          |");
                Console.WriteLine("|                                              |");
                Console.WriteLine("|    * *  *      W A R N I N G     *  * *      |");
                Console.WriteLine("|                                              |");
                Console.WriteLine("|         This is a private irc server         |");
                Console.WriteLine("|                                              |");
                Console.WriteLine("| If you're not a member of def4ult leave ASAP |");
                Console.WriteLine("| Users of this system have no expectation of  |");
                Console.WriteLine("| privacy. By continuing, you consent to your  |");
                Console.WriteLine("| keystrokes and data content being monitored  |");
                Console.WriteLine("|                                              |");
                Console.WriteLine("================================================");
                Console.WriteLine("|                  PLEASE WAIT                 |");
                Console.WriteLine("================================================");
                await client.ConnectAsync();



                while (true)
                {
                    string mesaj = Console.ReadLine();
                    if (mesaj == "/quit")
                    { disconnect("openvpn.exe"); break; }
                    await client.SendAsync(new PrivMsgMessage(channel, encrypt(mesaj)));
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("def4ult team - user: " + nickname + " : " + mesaj);


                }
                this.Visible = true;

                // Hide
                ShowWindow(handle, SW_HIDE);
                disconnect("openvpn.exe");
            }
        }

        private void ip_Tick(object sender, EventArgs e)
        {

            

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");

                request.UserAgent = "curl"; // this will tell the server to return the information as if the request was made by the linux "curl" command

                string publicIPAddress;

                request.Method = "GET";
                using (WebResponse response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        publicIPAddress = reader.ReadToEnd();
                    }
                }

            } catch { }
        }
        private void connect() //vpn
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = @"C:\Program Files\OpenVPN\bin\openvpn.exe";
            startInfo.Arguments = @"--config bin\def4ult.ovpn";
            startInfo.Verb = "runas";
            process.StartInfo = startInfo;
            process.Start();
        }
        private void disconnect(string processName) //vpn
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "taskkill",
                Arguments = $"/f /im {processName}",
                CreateNoWindow = true,
                UseShellExecute = false
            }).WaitForExit();
        }
        private void guna2Button3_Click(object sender, EventArgs e)
        {
            connect();

        }

        private void guna2Button3_Click_1(object sender, EventArgs e)
        {
            disconnect("openvpn.exe");
        }

        private void def4ult_Leave(object sender, EventArgs e)
        {
            disconnect("openvpn.exe");
        }

        private void def4ult_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnect("openvpn.exe");
        }

        private void def4ult_FormClosing(object sender, FormClosingEventArgs e)
        {
            disconnect("openvpn.exe");
        }

        private void guna2ControlBox1_Click(object sender, EventArgs e)
        {
            disconnect("openvpn.exe");
        }
    }

}

