using System;
using SoMRandomizer.config.settings;
using SoMRandomizer.processing.common;
using SoMRandomizer.processing.openworld;
using SoMRandomizer.util;

namespace MyApp
{
    internal class Program
    {
        static void Main(string[] cmdLine)
        {
            Console.WriteLine("Hello World!");
            
            // process commandline args for open world.  require all of these:
            // srcRom=""
            // dstRom=""
            // seed=""
            // options=""

            // note that this currently only supports open world mode, though it wouldn't be too hard to make it run for any mode.
            try
            {
                Dictionary<string, string> cmdArgsProcessed = CmdArgParser.processCmdArgs(cmdLine);
                if (!cmdArgsProcessed.ContainsKey("srcRom"))
                {
                    Console.WriteLine("missing srcRom=(path)");
                    Environment.Exit(1);
                }
                if (!cmdArgsProcessed.ContainsKey("dstRom"))
                {
                    Console.WriteLine("missing dstRom=(path)");
                    Environment.Exit(1);
                }
                if (!cmdArgsProcessed.ContainsKey("seed"))
                {
                    Console.WriteLine("missing seed=(value)");
                    Environment.Exit(1);
                }
                if (!cmdArgsProcessed.ContainsKey("options"))
                {
                    Console.WriteLine("missing options=(value)");
                    Environment.Exit(1);
                }

                // process individual options, similar to how OptionsManager does it for the UI
                string[] allEntries = cmdArgsProcessed["options"].Trim().Split(new char[] { ' ' });
                Dictionary<string, string> allEntriesMap = new Dictionary<string, string>();
                foreach (string entry in allEntries)
                {
                    string str = entry.Trim();
                    int equalsIndex = str.IndexOf('=');
                    List<string> values = new List<string>();
                    if (equalsIndex >= 0)
                    {
                        values.Add(str.Substring(0, equalsIndex));
                        values.Add(str.Substring(equalsIndex + 1));
                    }
                    if (values.Count == 2)
                    {
                        allEntriesMap[values[0]] = values[1];
                    }
                    else
                    {
                        Console.WriteLine("Unexpected string: " + entry);
                        Environment.Exit(1);
                    }
                }

                // create default settings and apply our overrides
                CommonSettings commonSettings = new CommonSettings();
                OpenWorldSettings openWorldSettings = new OpenWorldSettings(commonSettings);
                // set a few common options for the log that the UI normally sets
                commonSettings.set(CommonSettings.PROPERTYNAME_MODE, OpenWorldSettings.MODE_KEY);
                commonSettings.set(CommonSettings.PROPERTYNAME_ALL_ENTERED_OPTIONS, cmdArgsProcessed["options"]);
                commonSettings.set(CommonSettings.PROPERTYNAME_VERSION, RomGenerator.VERSION_NUMBER);

                openWorldSettings.processNewSettings(allEntriesMap);
                OpenWorldGenerator openWorldGenerator = new OpenWorldGenerator();
                Dictionary<string, RomGenerator> generatorsByRomType = new Dictionary<string, RomGenerator> { { OpenWorldSettings.MODE_KEY, openWorldGenerator } };
                Dictionary<string, RandoSettings> settingsByRomType = new Dictionary<string, RandoSettings> { { OpenWorldSettings.MODE_KEY, openWorldSettings } };
                // run rom generation
                // note there are no checks here for whether the dstRom exists - it will overwrite
                try
                {
                    RomGenerator.initGeneration(cmdArgsProcessed["srcRom"], cmdArgsProcessed["dstRom"], cmdArgsProcessed["seed"], generatorsByRomType, commonSettings, settingsByRomType);
                    Console.WriteLine("done!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: " + e.Message);
                }
            }
            catch(Exception ee)
            {
                Console.WriteLine("exception encountered: " + ee.Message);
            }
        }
    }
}
