using System;
using System.Collections.Generic;
using System.Diagnostics;
using APKToolGUI.Utils;
using Java;

namespace APKToolGUI
{
    public class DptShell : JarProcess
    {
        public event DptShellDataReceivedEventHandler DptShellOutputDataReceived;
        public event DptShellDataReceivedEventHandler DptShellErrorDataReceived;

        public DptShell(string javaPath, string jarPath)
            : base(javaPath, jarPath)
        {
            OutputDataReceived += OnOutputDataReceived;
            ErrorDataReceived += OnErrorDataReceived;
        }

        void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                DptShellErrorDataReceived?.Invoke(this, new DptShellDataReceivedEventArgs(e.Data));
        }

        void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                DptShellOutputDataReceived?.Invoke(this, new DptShellDataReceivedEventArgs(e.Data));
        }

        public int Obfuscate(string packageFile, string outputDirectory, DptShellOptions options)
        {
            List<string> args = new List<string>();

            if (options.Debug)
                args.Add("--debug");
            if (options.DisableAcf)
                args.Add("--disable-acf");
            if (options.DumpCode)
                args.Add("--dump-code");
            if (options.NoisyLog)
                args.Add("--noisy-log");
            if (options.Smaller)
                args.Add("--smaller");
            if (options.NoSign)
                args.Add("--no-sign");
            if (options.KeepClasses)
                args.Add("--keep-classes");

            if (!String.IsNullOrWhiteSpace(options.ExcludeAbi))
                args.Add($"--exclude-abi \"{options.ExcludeAbi}\"");
            if (!String.IsNullOrWhiteSpace(options.RulesFile))
                args.Add($"--rules-file \"{options.RulesFile}\"");
            if (!String.IsNullOrWhiteSpace(outputDirectory))
                args.Add($"--output \"{outputDirectory}\"");

            args.Add($"--package-file \"{packageFile}\"");

            Start(String.Join(" ", args));
            BeginOutputReadLine();
            BeginErrorReadLine();
            WaitForExit();
            CancelOutputRead();
            CancelErrorRead();

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
            catch { }
        }
    }

    public class DptShellOptions
    {
        public bool Debug { get; set; }
        public bool DisableAcf { get; set; }
        public bool DumpCode { get; set; }
        public bool NoisyLog { get; set; }
        public bool Smaller { get; set; }
        public bool NoSign { get; set; }
        public bool KeepClasses { get; set; }
        public string ExcludeAbi { get; set; }
        public string RulesFile { get; set; }
    }

    public delegate void DptShellDataReceivedEventHandler(object sender, DptShellDataReceivedEventArgs e);

    public class DptShellDataReceivedEventArgs : EventArgs
    {
        public string Message { get; private set; }

        public DptShellDataReceivedEventArgs(string message)
        {
            Message = message;
        }
    }
}
