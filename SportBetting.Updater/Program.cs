using System;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace SportBetting.Updater
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("UPDATING...");

            string[] inArgs = args[0].Split('|');

            //1 sec delay to allow main app to close
            Thread.Sleep(1000);

            string sUpdatePath = Directory.GetParent(Environment.CurrentDirectory.ToString()).ToString() + "\\update";
            string sParentDir = Directory.GetParent(sUpdatePath).ToString();
            string sCurrentDir = Environment.CurrentDirectory.ToString();

            PriorProcess(inArgs[0]);

            foreach (string subDir in Directory.GetDirectories(sCurrentDir))
            {
                if (subDir.Contains("log"))
                    continue;
                if (subDir.Contains("Config"))
                    continue;
                if (subDir.Contains("printed tickets"))
                    continue;
                if (subDir.Contains("printer objects"))
                    continue;
                try
                {
                    Directory.Delete(subDir, true);
                    Console.WriteLine("deleted: " + subDir);
                }
                catch (Exception)
                {
                }

            }


            foreach (string filename in Directory.GetFiles(sCurrentDir))
            {
                if (filename.Contains("_restartLog"))
                    continue;
                if (filename.Contains("SportBetting.Updater.exe"))
                    continue;
                if (filename.Contains("cert.p12"))
                    continue;
                try
                {
                    File.Delete(filename);
                    Console.WriteLine("deleted: " + filename);

                }
                catch (Exception)
                {
                }
            }

            foreach (string subDir in Directory.GetDirectories(sUpdatePath))
            {
                string sDirX = subDir.Replace(sUpdatePath, "");
                CopyDirectory(subDir, sCurrentDir + sDirX);
                Console.WriteLine("UPDATED: " + sCurrentDir + sDirX);
            }

            CopyDirectory(sUpdatePath, sCurrentDir);

            File.WriteAllText(Directory.GetCurrentDirectory() + "\\version.txt", inArgs[2]);
            Console.WriteLine("VERSION FILE CREATED. VERSION: " + inArgs[2]);

            try
            {
                // copy DOWNLOADER files to temp files
                File.Copy(sUpdatePath + "\\SportBetting.Updater.exe", sCurrentDir + "\\SportBetting.Updater1.exe");
                File.Copy(sUpdatePath + "\\SportBetting.Updater.pdb", sCurrentDir + "\\SportBetting.Updater1.pdb");

                Directory.Delete(sUpdatePath, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("* * * * * ERROR! * * * * *");
                Console.WriteLine(e.Message);
                Thread.Sleep(5000);
            }

            Console.WriteLine("UPDATE SUCCESSFULL. RESTARTING...");
            Thread.Sleep(2000);

            Process.Start(Directory.GetCurrentDirectory() + "\\" + inArgs[1]);
        }

        public static void PriorProcess(string sProcessName)
        {
            Process[] procs = Process.GetProcessesByName(sProcessName);
            foreach (Process p in procs)
            {
                p.Kill();
                Console.WriteLine("OLD INSTANCE KILLED");
            }
        }

        /// <summary>
        /// copies content of current direcory to new directory
        /// </summary>
        /// <param name="oldDir">String containing name of old directory</param> 
        /// <param name="newDir">String containing name of new directory</param> 

        private static void CopyDirectory(string oldDir, string newDir)
        {

            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }

            string[] files = Directory.GetFiles(oldDir);

            foreach (string file in files)
            {
                string destFile = Path.Combine(newDir, Path.GetFileName(file));

                try
                {
                    // don't copy pref file
                    if (!file.Contains("PrefFile.txt"))
                        File.Copy(file, destFile, true);
                }
                catch (Exception ex)
                {
                    // just do nothing since most probably the file can't be copied because it is used by another process
                    //      which is current Updater and 3-4 additional files.
                }

            }
            string[] dirs = Directory.GetDirectories(oldDir);
            //alle Unterverzeichnisse kopieren
            foreach (string dir in dirs)
            {
                string destDir = dir.Replace(oldDir.TrimEnd(Path.DirectorySeparatorChar), newDir.TrimEnd(Path.DirectorySeparatorChar));

                CopyDirectory(dir, destDir);
            }
        }
    }
}
