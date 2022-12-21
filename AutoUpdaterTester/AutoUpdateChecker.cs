using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater
{
    public class AutoUpdateChecker
    {
        public static void RunChecker()
        {
            //AutoUpdaterLib.Updater.CheckAndDownloadUpdates();

            bool updatesFound = AutoUpdaterLib.Updater.CheckForUpdates();

            Thread thread = new Thread(RepeatPhrase);
            thread.Start();

            if (updatesFound)
            {
                DialogResult result = System.Windows.Forms.MessageBox.Show("A new update has been found. Do you want to update?", "Update Found", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

                if (result == DialogResult.Yes)
                {
                    var proc1 = new ProcessStartInfo();
                    proc1.UseShellExecute = true;
                    proc1.CreateNoWindow = false;
                    proc1.WorkingDirectory = @"";
                    proc1.Arguments = "\"autoupdater.dll\"";
                    proc1.FileName = "dotnet.exe";
                    Process.Start(proc1);

                    System.Environment.Exit(1);
                }
            }

        }

        public static void RepeatPhrase()
        {
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("Repeating Phrase.");
                Thread.Sleep(1000);
            }
        }
    }
}