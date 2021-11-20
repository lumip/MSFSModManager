using System;

namespace MSFSModManager.Core.PackageSources
{

    public interface IProgressMonitor
    {
        void DownloadStarted(IDownloadProgressMonitor monitor);
        void ExtractionStarted(string packageId, IVersionNumber versionNumber);
        void ExtractionCompleted(string packageId, IVersionNumber versionNumber);
    }

}