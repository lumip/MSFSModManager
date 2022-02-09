// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using ReactiveUI;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.GUI.ViewModels
{
    class DependencyResolutionPageViewModel : ViewModelBase, IProgressMonitor
    {

        private string _statusLabel;
        public string StatusLabel
        {
            get => _statusLabel;
            set => this.RaiseAndSetIfChanged(ref _statusLabel, value);
        }

        public DependencyResolutionPageViewModel()
        {
            _statusLabel = "";
        }

        public void DownloadStarted(IDownloadProgressMonitor monitor)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Downloading {monitor.PackageId} v{monitor.Version}... ";
            }
            monitor.DownloadProgress += DownloadProgress;
        }

        private void DownloadProgress(IDownloadProgressMonitor monitor)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Downloading {monitor.PackageId} v{monitor.Version}... {monitor.CurrentSize}/{monitor.TotalSize}";
            }
        }

        public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Extracting {packageId} to cache completed; reading manifest...";
            }
        }

        public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Extracting {packageId} v{versionNumber} to cache to read manifest...";
            }
        }

        public void RequestPending(string packageId)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Looking up {packageId}...";
            }
        }

        public void CopyingStarted(string packageId, IVersionNumber versionNumber)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Copying {packageId}...";
            }
        }

        public void CopyingCompleted(string packageId, IVersionNumber versionNumber)
        {
            lock (StatusLabel)
            {
                StatusLabel = $"Copying {packageId} completed.";
            }
        }
    }
}
