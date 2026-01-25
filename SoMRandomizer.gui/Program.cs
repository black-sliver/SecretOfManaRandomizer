/*
    Secret of Mana Randomizer
    Copyright (C) 2024  Moppleton

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
    USA
 */

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SoMRandomizer.gui
{
    /// <summary>
    /// Main entry point for mana rando app
    /// </summary>
    /// 
    /// <remarks>Author: Moppleton</remarks>
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        private const int ATTACH_PARENT_PROCESS = -1;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            string[] cmdLine = Environment.GetCommandLineArgs();
            // arg [0] is the path to the binary
            // if any args are passed, process as a command-line rom generate and don't open the UI
            if (cmdLine.Length <= 1)
            {
                GuiApp.Run();
            }
            else
            {
                // need to call this for console to appear in windows env
                try
                {
                    AttachConsole(ATTACH_PARENT_PROCESS);
                }
                catch
                {
                    // ignore
                }
                ConsoleApp.Run(cmdLine, 1);
            }
        }

        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName assemblyName = new AssemblyName(args.Name);

            var path = assemblyName.Name + ".dll";
            if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false) path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null) return null;

                var assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }
    }
}
