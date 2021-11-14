using System;

namespace MSFSModManager.Core.PackageSources
{
    public delegate void DownloadProgressHandler(IDownloadProgressMonitor monitor);
    public interface IDownloadProgressMonitor
    {
        string PackageId { get; }
        VersionNumber Version { get; }

        long TotalSize { get; }
        long CurrentSize { get; }
        float CurrentPercentage { get; }

        object? UserData { get; set; }

        event DownloadProgressHandler? DownloadProgress;
    }

    public delegate void DownloadStartedHandler(IDownloadProgressMonitor monitor);

    public delegate void ExtractionStartedHandler(string packageId, VersionNumber versionNumber);
    public delegate void ExtractionCompletedHandler(string packageId, VersionNumber versionNumber);
}