using System;

namespace MSFSModManager.Core.PackageSources
{
    public class HttpClientDownloadProgressMonitor : IDownloadProgressMonitor, IProgress<long>
    {
        public object? UserData { get; set; }

        public string PackageId { get; }

        public VersionNumber Version { get; }

        public long TotalSize { get; }

        public long CurrentSize { get; private set; }

        public float CurrentPercentage => 100f * ((float)CurrentSize / (float)TotalSize);

        public bool IsCompleted => CurrentSize == TotalSize;

        public HttpClientDownloadProgressMonitor(string packageId, VersionNumber version, long totalSize)
        {
            PackageId = packageId;
            Version = version;
            TotalSize = totalSize;
            CurrentSize = 0;
        }

        public void Report(long progress)
        {
            CurrentSize = progress;
            DownloadProgress?.Invoke(this);
        }

        public event DownloadProgressHandler? DownloadProgress;
    }
}