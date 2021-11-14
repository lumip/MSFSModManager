using System;

namespace MSFSModManager.Core.PackageSources
{

    public interface IProgressMonitor
    {
        void DownloadStarted(IDownloadProgressMonitor monitor);
        void ExtractionStarted(string packageId, VersionNumber versionNumber);
        void ExtractionCompleted(string packageId, VersionNumber versionNumber);
    }

}