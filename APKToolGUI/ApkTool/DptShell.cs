using System;
using System.Diagnostics;
using APKToolGUI.Utils;
using Java;

namespace APKToolGUI
{
    public class DptShell : JarProcess
    {
        public DptShell(string javaPath, string jarPath)
            : base(javaPath, jarPath)
        {
        }

        public int Obfuscate(string arguments)
        {
            Start(arguments);
            BeginOutputReadLine();
            BeginErrorReadLine();
            WaitForExit();
            return ExitCode;
        }

        public void Cancel()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("java"))
                {
                    if (process.Id == Id)
                    {
                        ProcessUtils.KillAllProcessesSpawnedBy((uint)Id);
                        process.Kill();
                    }
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
