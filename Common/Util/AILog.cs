using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.AI
{
    public static class AILog
    {
        public static bool SuppressLog = false;

        /// <summary>Prints the message to the console.</summary>
        public static void Log(string message)
        {
            if (!SuppressLog)
                Console.Error.WriteLine(DateTime.Now + ": " + message);
        }


    }
}
