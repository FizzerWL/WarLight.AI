using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{
    public static class AILog
    {
        public static Func<string, bool> DoLog = null;

        public static void Log(string area, string message)
        {
            if (DoLog == null || DoLog(area))
                Console.Error.WriteLine(DateTime.Now + " " + area + ": " + message);
        }


    }
}
