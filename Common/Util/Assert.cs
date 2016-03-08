using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarLight.Shared.AI
{

    public static class Assert
    {
        public static void Fatal(bool cond, string msg = "Assert failed")
        {
            if (!cond)
            {
                if (Debugger.IsAttached)
                    Debugger.Break();
                throw new AssertFailedException(msg);
            }
        }

        public static void Contains(string inputString, params string[] contains)
        {
            foreach (var c in contains)
                Fatal(inputString.Contains(c), inputString + " did not contain " + c);
        }
        public static void ContainsOneOf(string inputString, params string[] contains)
        {
            foreach (var c in contains)
                if (inputString.Contains(c))
                    return;
            Fatal(false, inputString + " did not contain any passed strings");
        }


    }

    public class AssertFailedException : Exception
    {
        public AssertFailedException(string msg)
            : base(msg)
        {
        }
    }
}
