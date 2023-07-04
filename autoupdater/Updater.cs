using AutoUpdater;
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
        private static string appdata = string.Empty;

        private static bool outputDebugInfo = true;

        internal static void RunUpdater()
        {
            string[] args = Environment.GetCommandLineArgs();
            CheckAndDownloadUpdates(args[1], args[2], args[3], args[4], args[5]);
        }

        public static void CheckAndDownloadUpdates(string applicationName, string updateServerURL, string versionFileName, string updateListFileName, string executableFileName)
        {
            UpdaterDebug.WriteLine("Running update with debug output:");
            UpdaterDebug.WriteLine(applicationName);
            UpdaterDebug.WriteLine(updateServerURL);
            UpdaterDebug.WriteLine(versionFileName);
            UpdaterDebug.WriteLine(updateListFileName);
            UpdaterDebug.WriteLine(executableFileName + "\n");
            Thread.Sleep(200);

            Console.WriteLine("Checking for Updates");
            bool updatesFound = CheckForUpdates(applicationName, updateServerURL, versionFileName);

            if (updatesFound)
            {
                DownloadUpdate(updateServerURL, versionFileName, updateListFileName);
                Console.WriteLine("Download Complete!");

                var proc1 = new ProcessStartInfo();
                proc1.UseShellExecute = true;
                proc1.CreateNoWindow = false;
                proc1.WorkingDirectory = @"";
                proc1.Arguments = "";
                proc1.FileName = executableFileName;
                Process.Start(proc1);
            }
            else
            {
                Console.WriteLine("Unknown error");
            }
            Thread.Sleep(5000000);
        }

        public static bool CheckForUpdates(string applicationName, string updateServerURL, string versionFileName)
        {
            //System.Diagnostics.Debugger.Break();

            appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Keanine\\AutoUpdater\\" + applicationName + "\\";

            Directory.CreateDirectory(appdata);

            string newIniFilePath = DownloadFile(updateServerURL, versionFileName, appdata, versionFileName);

            AutoUpdater.UpdaterDebug.WriteLine("Downloaded file: " + newIniFilePath);

            if (newIniFilePath == string.Empty)
                return false;

            IniUtility newIniFile = new IniUtility(newIniFilePath);

            if (File.Exists(versionFileName))
            {
                IniUtility existingIniFile = new IniUtility(versionFileName);

                string oldVersionStr = existingIniFile.Read("Version", "Main");
                string newVersionStr = newIniFile.Read("Version", "Main");

                AutoUpdater.UpdaterDebug.WriteLine("Old INI: \"" + oldVersionStr + "\"");
                AutoUpdater.UpdaterDebug.WriteLine("New INI: " + newVersionStr);

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
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"No previous version of {applicationName} found.");
                return true;
            }

            Console.WriteLine($"Error: The update server could not be contacted or the versions could not be determined");
            return false;
            //throw new System.Exception("The update server could not be contacted");
        }

        ///// <summary>
        ///// Download a file from a base URL and a filename
        ///// </summary>
        ///// <param name="url"></param>
        ///// <param name="fileName"></param>
        ///// <returns>Path of the downloaded file</returns>
        //public static string DownloadFile(string baseURL, string fileName, string outputFolder, string outputName)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.BaseAddress = new Uri(baseURL);
        //        string result = client.GetStringAsync(fileName).Result;

        //        string outputPath = Path.Combine(outputFolder, fileName);
        //        File.WriteAllText(outputPath, result);
        //        return outputPath;
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
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(baseURL);
                    byte[] result = client.GetByteArrayAsync(fileName).Result;

                    string outputPath = Path.Combine(outputFolder, outputName);
                    File.WriteAllBytes(outputPath, result);
                    return outputPath;
                }
            }
            catch
            {
                UpdaterDebug.WriteLine("Could not find " + Path.Combine(outputFolder, outputName));
                return string.Empty;
            }
        }

        public static void DownloadUpdate(string updateServerURL, string versionFileName, string updateListFileName)
        {
            //System.Diagnostics.Debugger.Break();

            if (File.Exists(versionFileName)) File.Delete(versionFileName);
            File.Move(Path.Combine(appdata, versionFileName), versionFileName);

            UpdaterDebug.WriteLine("Downloading updateList: " + updateListFileName);
            string updateListFilePath = DownloadFile(updateServerURL, updateListFileName, appdata, updateListFileName);
            string[] updateList = File.ReadAllLines(updateListFilePath);

            foreach (var fileName in updateList)
            {
                if (fileName.StartsWith("delete "))
                {
                    UpdaterDebug.WriteLine("Deleting: " + fileName);
                    string file = fileName.Substring("delete ".Length);
                    if (File.Exists(file))
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    UpdaterDebug.WriteLine("Downloading: " + fileName);
                    DownloadFile(updateServerURL, fileName, string.Empty, fileName);
                }
            }
        }
    }
}
