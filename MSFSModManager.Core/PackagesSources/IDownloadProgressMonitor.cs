using System;

namespace MSFSModManager.Core.PackageSources
{
    public delegate void DownloadProgressHandler(IDownloadProgressMonitor monitor);
    public interface IDownloadProgressMonitor
    {
        string PackageId { get; }
        IVersionNumber Version { get; }

        long TotalSize { get; }
        long CurrentSize { get; }
        float CurrentPercentage { get; }

        object? UserData { get; set; }

        event DownloadProgressHandler? DownloadProgress;
    }

    public delegate void DownloadStartedHandler(IDownloadProgressMonitor monitor);

    public delegate void ExtractionStartedHandler(string packageId, IVersionNumber versionNumber);
    public delegate void ExtractionCompletedHandler(string packageId, IVersionNumber versionNumber);
}