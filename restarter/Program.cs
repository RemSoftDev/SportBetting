using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace restarter
{
    class Program
    {
        static void Main(string[] args)
        {
            String testMode = String.Empty;
            foreach (var s in args)
            {
                Console.WriteLine(s);
                //int argValue = 0;
                //Int32.TryParse(s, out argValue);
                string argValue = s.ToString(CultureInfo.InvariantCulture);
                if (argValue == "testmode")
                {
                    testMode = "testmode";
                    Console.WriteLine("Starting in TestMode");
                }
            }

            bool keepRunning = false;
            var id = Convert.ToInt32(args[1]);
            do
            {
                keepRunning = false;
                Process[] processlist = Process.GetProcesses();
                foreach (Process theprocess in processlist)
                {
                    if (theprocess.Id == id)
                    {
                        keepRunning = true;
                        Thread.Sleep(1000);
                        break;
                    }
                }


            } while (keepRunning);
            Console.WriteLine("starting application");
            Thread.Sleep(2000);
            Process proc = new Process();
            proc.StartInfo.FileName = args[0];
            proc.StartInfo.Arguments = testMode;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.Start();

        }

        static bool IsSingleInstance()
        {
            try
            {
                // Try to open existing mutex.
                Mutex.OpenExisting("SportBetting");
            }
            catch
            {

                // Only one instance.
                return true;
            }
            // More than one instance.
            return false;
        }
    }
}
