using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.GUI
{
    public class LogProgressMonitor : IProgressMonitor
    {
        public void CopyingCompleted(string packageId, IVersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Info, $"Copying {packageId} v{versionNumber}...");
        }

        public void CopyingStarted(string packageId, IVersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Info, $"Copying completed: {packageId} v{versionNumber} .");
        }

        public void DownloadStarted(IDownloadProgressMonitor monitor)
        {
            GlobalLogger.Log(LogLevel.Info, $"Downloading {monitor.PackageId} v{monitor.Version}...");
            monitor.DownloadProgress += m => {
                if (m.CurrentSize == m.TotalSize) GlobalLogger.Log(LogLevel.Info, $"Download completed: {m.PackageId} v{m.Version} .");
            };
        }

        public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Info, $"Extraction completed: {packageId} v{versionNumber} .");
        }

        public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Info, $"Extracting {packageId} v{versionNumber}...");
        }

        public void RequestPending(string packageId)
        {
            
        }
    }
}