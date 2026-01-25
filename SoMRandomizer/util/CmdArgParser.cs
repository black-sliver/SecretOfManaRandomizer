using System;
using System.Collections.Generic;

namespace SoMRandomizer.util
{
    /// <summary>
    /// Utility to process command-line arguments of the form key=value into a dictionary of the keys to the values.
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    public static class CmdArgParser
    {
        public static Dictionary<string, string> ProcessCmdArgs(string[] args, int offset)
        {
            // NOTE: if args is coming from Environment, [0] is EXE, if it's coming from Main(), [0] is the first arg
            Dictionary<string, string> processed = new Dictionary<string, string>();
            for (int i=offset; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
                int equalsIndex = args[i].IndexOf('=');
                if(equalsIndex > 0)
                {
                    string argName = args[i].Substring(0, equalsIndex);
                    string argValue = args[i].Substring(equalsIndex + 1);
                    processed[argName] = argValue;
                }
            }
            return processed;
        }
    }
}
