using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MindLated.Protection.Anti.Runtime
{
    internal class SelfDeleteClass
    {
        public static void Init()
        {
            if (IsSandboxie())
                SelfDelete();
            if (IsDebugger())
                SelfDelete();
            if (IsdnSpyRun())
                SelfDelete();
        }

        private static bool IsSandboxie()
        {
            return IsDetected();
        }

        private static bool IsDebugger()
        {
            return Run();
        }

        private static bool IsdnSpyRun()
        {
            return ValueType();
        }

        private static void SelfDelete()
        {
            Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + Assembly.GetExecutingAssembly().Location + "\"") { WindowStyle = ProcessWindowStyle.Hidden })?.Dispose();
            Process.GetCurrentProcess().Kill();
        }

        private static bool ValueType()
        {
            return File.Exists(Environment.ExpandEnvironmentVariables("%appdata%") + "\\dnSpy\\dnSpy.xml");
        }

        private static IntPtr GetModuleHandle(string libName)
        {
            foreach (ProcessModule pMod in Process.GetCurrentProcess().Modules)
                if (pMod.ModuleName.ToLower().Contains(libName.ToLower()))
                    return pMod.BaseAddress;
            return IntPtr.Zero;
        }

        private static bool IsDetected()
        {
            return GetModuleHandle("SbieDll.dll") != IntPtr.Zero;
        }

        private static bool Run()
        {
            var returnvalue = false;
            if (Debugger.IsAttached || Debugger.IsLogging())
            {
                returnvalue = true;
            }
            else
            {
                var strArray = new[] { "codecracker", "x32dbg", "x64dbg", "ollydbg", "ida", "charles", "dnspy", "simpleassembly", "peek", "httpanalyzer", "httpdebug", "fiddler", "wireshark", "dbx", "mdbg", "gdb", "windbg", "dbgclr", "kdb", "kgdb", "mdb", "processhacker", "scylla_x86", "scylla_x64", "scylla", "idau64", "idau", "idaq", "idaq64", "idaw", "idaw64", "idag", "idag64", "ida64", "ida", "ImportREC", "IMMUNITYDEBUGGER", "MegaDumper", "CodeBrowser", "reshacker", "cheat engine" };
                foreach (var process in Process.GetProcesses())
                    if (process != Process.GetCurrentProcess())
                        for (var index = 0; index < strArray.Length; ++index)
                        {
                            if (process.ProcessName.ToLower().Contains(strArray[index])) returnvalue = true;

                            if (process.MainWindowTitle.ToLower().Contains(strArray[index])) returnvalue = true;
                        }
            }
            return returnvalue;
        }
    }
}