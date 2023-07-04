using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdater
{
    internal class UpdaterDebug
    {
        public static bool enableDebugMessages = true;

        public static void WriteLine(string message)
        {
            if (enableDebugMessages)
            {
                Console.WriteLine(message);
            }
        }
    }
}
