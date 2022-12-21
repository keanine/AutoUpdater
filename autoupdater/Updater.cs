using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdaterLib
{
    public class Updater
    {
        private static string applicationName = "UpdaterTesting";
        private static string updateServerURL = @"https://raw.githubusercontent.com/keanine/AutoUpdater/main/UpdateServer/";
        private static string updateFileName = "version.ini";
        private static string updateListFileName = "updatelist.txt";
        private static string executableFileName = "AutoUpdaterTester.exe";

        private static string appdata = string.Empty;


        public static void CheckAndDownloadUpdates()
        {
            Thread.Sleep(500);

            Console.WriteLine("Checking for Updates");
            bool updatesFound = CheckForUpdates();

            if (updatesFound)
            {
                DownloadUpdate();
                Console.WriteLine("Download Complete!");

                var proc1 = new ProcessStartInfo();
                proc1.UseShellExecute = true;
                proc1.CreateNoWindow = false;
                proc1.WorkingDirectory = @"";
                proc1.Arguments = "";
                proc1.FileName = executableFileName;
                Process.Start(proc1);

                //System.Environment.Exit(1);
            }
        }

        public static bool CheckForUpdates()
        {
            //System.Diagnostics.Debugger.Break();

            appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Keanine\\AutoUpdater\\" + applicationName + "\\";

            Directory.CreateDirectory(appdata);

            string newIniFilePath = DownloadFile(updateServerURL, updateFileName, appdata, updateFileName);
            IniUtility newIniFile = new IniUtility(newIniFilePath);

            if (File.Exists(updateFileName))
            {
                IniUtility existingIniFile = new IniUtility(updateFileName);

                string oldVersionStr = existingIniFile.Read("Version", "Main");
                string newVersionStr = newIniFile.Read("Version", "Main");

                float oldVersionNumber = float.Parse(System.Text.RegularExpressions.Regex.Match(oldVersionStr, @"[+-]?([0-9]*[.])?[0-9]+").Value);
                float newVersionNumber = float.Parse(System.Text.RegularExpressions.Regex.Match(newVersionStr, @"[+-]?([0-9]*[.])?[0-9]+").Value);

                if (newVersionNumber == oldVersionNumber)
                {
                    //if the letters in the number are "higher" then its newer
                    Console.WriteLine($"{applicationName} is already up to date, Version {oldVersionStr}");
                    return false;
                }
                else if (newVersionNumber < oldVersionNumber)
                {
                    Console.WriteLine($"Error. Local version is {oldVersionStr} but the latest version of {applicationName} is {newVersionStr}");
                    return false;
                }
                else if (newVersionNumber > oldVersionNumber)
                {
                    Console.WriteLine("New version found.");
                    //DownloadUpdate();
                    //File.Move(newIniFilePath, updateFileName, true);
                    //Console.WriteLine($"{applicationName} Updated Successfully");
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"No previous version of {applicationName} found.");
                //DownloadUpdate();
                //File.Move(newIniFilePath, updateFileName, true);
                //Console.WriteLine($"{applicationName} Downloaded Successfully");
                return true;
            }

            throw new System.Exception("The update server could not be contacted");
        }

        //public static void CheckForUpdates()
        //{
        //    appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Keanine\\AutoUpdater\\" + applicationName + "\\";

        //    Directory.CreateDirectory(appdata);

        //    string newIniFilePath = DownloadFile(updateServerURL, updateFileName, appdata, updateFileName);
        //    IniUtility newIniFile = new IniUtility(newIniFilePath);

        //    if (File.Exists(updateFileName))
        //    {
        //        IniUtility existingIniFile = new IniUtility(updateFileName);

        //        string oldVersionStr = existingIniFile.Read("Version", "Main");
        //        string newVersionStr = newIniFile.Read("Version", "Main");

        //        float oldVersionNumber = float.Parse(System.Text.RegularExpressions.Regex.Match(oldVersionStr, @"[+-]?([0-9]*[.])?[0-9]+").Value);
        //        float newVersionNumber = float.Parse(System.Text.RegularExpressions.Regex.Match(newVersionStr, @"[+-]?([0-9]*[.])?[0-9]+").Value);

        //        if (newVersionNumber == oldVersionNumber)
        //        {
        //            //if the letters in the number are "higher" then its newer
        //                Console.WriteLine($"{applicationName} is already up to date, Version {oldVersionStr}");
        //        }
        //        else if (newVersionNumber < oldVersionNumber)
        //        {
        //            Console.WriteLine($"Error. Local version is {oldVersionStr} but the latest version of {applicationName} is {newVersionStr}");
        //        }
        //        else if(newVersionNumber > oldVersionNumber)
        //        {
        //            Console.WriteLine("New version found. Updating...");
        //            DownloadUpdate();
        //            // Download the new version, then
        //            File.Move(newIniFilePath, updateFileName, true);
        //            Console.WriteLine($"{applicationName} Updated Successfully");
        //        }
        //    }
        //    else
        //    {
        //        Console.WriteLine($"No previous version of {applicationName} found. Downloading...");
        //        DownloadUpdate();
        //        File.Move(newIniFilePath, updateFileName, true);
        //        Console.WriteLine($"{applicationName} Downloaded Successfully");
        //    }
        //}

        /// <summary>
        /// Download a file from a base URL and a filename
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fileName"></param>
        /// <returns>Path of the downloaded file</returns>
        public static string DownloadFile(string baseURL, string fileName, string outputFolder, string outputName)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(baseURL);
                string result = client.GetStringAsync(fileName).Result;

                string outputPath = Path.Combine(outputFolder, fileName);
                File.WriteAllText(outputPath, result);
                return outputPath;
            }
        }

        public static void DownloadUpdate()
        {
            //System.Diagnostics.Debugger.Break();

            if (File.Exists(updateFileName)) File.Delete(updateFileName);
            File.Move(Path.Combine(appdata, updateFileName), updateFileName);

            string updateListFilePath = DownloadFile(updateServerURL, updateListFileName, appdata, updateListFileName);
            string[] updateList = File.ReadAllLines(updateListFilePath);

            foreach (var fileName in updateList)
            {
                DownloadFile(updateServerURL, fileName, string.Empty, fileName);
            }
        }
    }
}
