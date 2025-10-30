using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;

namespace APKToolGUI.Utils
{
    internal static class DptShellResourceManager
    {
        private const string DownloadUrl = "https://github.com/luoyesiqiu/dpt-shell/releases/download/v2.5.0/dpt-shell-v2.5.0.zip";
        private const string ZipFileName = "dpt-shell-v2.5.0.zip";
        private const string ZipRootPrefix = "executable/";
        private static readonly object SyncLock = new object();

        internal static bool EnsureResources(Action<string> statusLog = null, Action<string> errorLog = null)
        {
            lock (SyncLock)
            {
                try
                {
                    if (ResourcesAvailable())
                        return true;

                    statusLog?.Invoke("Downloading dpt-shell components...");

                    string resourcesDirectory = Path.GetDirectoryName(Program.DPT_PATH);
                    if (String.IsNullOrEmpty(resourcesDirectory))
                        return false;

                    Directory.CreateDirectory(resourcesDirectory);

                    string tempRoot = Path.Combine(Path.GetTempPath(), "APKToolGUI", "dpt-shell");
                    Directory.CreateDirectory(tempRoot);
                    string zipPath = Path.Combine(tempRoot, ZipFileName);

                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(DownloadUrl, zipPath);
                        }

                        ExtractResources(zipPath, resourcesDirectory);
                    }
                    finally
                    {
                        if (File.Exists(zipPath))
                            File.Delete(zipPath);
                    }

                    statusLog?.Invoke("dpt-shell components are ready.");
                    return ResourcesAvailable();
                }
                catch (Exception ex)
                {
                    errorLog?.Invoke($"Failed to prepare dpt-shell: {ex.Message}");
                    return false;
                }
            }
        }

        private static bool ResourcesAvailable()
        {
            if (!File.Exists(Program.DPT_PATH))
                return false;

            string shellFilesPath = Program.DptShellFilesPath;
            if (!Directory.Exists(shellFilesPath))
                return false;

            return Directory.EnumerateFileSystemEntries(shellFilesPath).Any();
        }

        private static void ExtractResources(string zipPath, string destinationRoot)
        {
            string shellFilesPath = Program.DptShellFilesPath;
            if (Directory.Exists(shellFilesPath))
                Directory.Delete(shellFilesPath, true);

            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(ZipRootPrefix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    string relativePath = entry.FullName.Substring(ZipRootPrefix.Length);
                    string destinationPath = Path.Combine(destinationRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

                    if (entry.FullName.EndsWith("/"))
                    {
                        Directory.CreateDirectory(destinationPath);
                        continue;
                    }

                    string directoryName = Path.GetDirectoryName(destinationPath);
                    if (!String.IsNullOrEmpty(directoryName))
                        Directory.CreateDirectory(directoryName);

                    entry.ExtractToFile(destinationPath, true);
                }
            }
        }
    }
}
