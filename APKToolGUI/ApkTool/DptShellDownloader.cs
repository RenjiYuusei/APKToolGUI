using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using APKToolGUI.Languages;

namespace APKToolGUI.ApkTool
{
    internal static class DptShellDownloader
    {
        internal const string DownloadUrl = "https://github.com/luoyesiqiu/dpt-shell/releases/download/v2.5.0/dpt-shell-v2.5.0.zip";

        private static readonly string[] CandidateJarNames = new[]
        {
            "dpt-shell.jar",
            "dpt.jar"
        };

        internal static bool TryEnsureToolAvailable(string destinationPath, Action<string> infoLogger, Action<string> errorLogger)
        {
            if (String.IsNullOrWhiteSpace(destinationPath))
                throw new ArgumentException("Destination path must be provided.", nameof(destinationPath));

            if (File.Exists(destinationPath))
                return true;

            infoLogger?.Invoke(String.Format(Language.ObfuscateDownloadingTool, DownloadUrl));

            string tempDirectory = Path.Combine(Path.GetTempPath(), "APKToolGUI", "dpt-shell", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDirectory);

            string archivePath = Path.Combine(tempDirectory, "dpt-shell.zip");

            try
            {
                DownloadArchiveAsync(archivePath).GetAwaiter().GetResult();
                ExtractJar(archivePath, destinationPath);
                infoLogger?.Invoke(String.Format(Language.ObfuscateDownloadSucceeded, destinationPath));
                return true;
            }
            catch (Exception ex)
            {
                errorLogger?.Invoke(String.Format(Language.ObfuscateDownloadFailed, ex.Message));
                return false;
            }
            finally
            {
                TryDeleteDirectory(tempDirectory);
            }
        }

        private static async Task DownloadArchiveAsync(string archivePath)
        {
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(DownloadUrl, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                using (var fileStream = File.Create(archivePath))
                {
                    await stream.CopyToAsync(fileStream).ConfigureAwait(false);
                }
            }
        }

        private static void ExtractJar(string archivePath, string destinationPath)
        {
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var entry = CandidateJarNames
                    .Select(candidate => archive.Entries.FirstOrDefault(e => String.Equals(Path.GetFileName(e.FullName), candidate, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(found => found != null);

                if (entry == null)
                    entry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".jar", StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                    throw new InvalidOperationException(Language.ObfuscateDownloadArchiveMissingJar);

                string directory = Path.GetDirectoryName(destinationPath);
                if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                entry.ExtractToFile(destinationPath, true);
            }
        }

        private static void TryDeleteDirectory(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory, true);
            }
            catch
            {
            }
        }
    }
}
