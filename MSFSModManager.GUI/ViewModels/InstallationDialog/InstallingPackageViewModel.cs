// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System.Diagnostics;

using ReactiveUI;
using System.Threading.Tasks;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using Avalonia.Media;

namespace MSFSModManager.GUI.ViewModels
{
    enum InstallationState
    {
        Pending,
        Installing,
        Faulted,
        Success
    }

    class InstallingPackageViewModel : ReactiveObject, IProgressMonitor
    {
        private PackageManifest _manifest;

        public string Id => _manifest.Id;

        public string Version =>
            _manifest.SourceVersion.Equals(_manifest.Version) ?
                $"{_manifest.SourceVersion}" :  $"{_manifest.SourceVersion} ({_manifest.Version})";

        private Task? _installationTask;
        public Task? InstallationTask
        {
            get => _installationTask;
            set
            {
                this.RaiseAndSetIfChanged(ref _installationTask, value);
                if (value != null)
                {
                    State = InstallationState.Installing;
                    value.ContinueWith(
                        t => {
                            IsIndeterminate = false;
                            if (t.IsFaulted || t.IsCanceled)
                            {
                                CurrentProgress = 0;
                                State = InstallationState.Faulted;
                                StatusLabel = "Error!";
                                StatusLabelColor = Brushes.Red;
                            }
                            else
                            {
                                State = InstallationState.Success;
                                StatusLabel = "Completed!";
                                CurrentProgress = 100;
                            }
                            TotalProgress = 100;
                            StatusLabelFontWeight = FontWeight.Bold;
                        }
                    );
                }
            }
        }

        private IBrush _statusLabelColor;
        public IBrush StatusLabelColor
        {
            get => _statusLabelColor;
            set => this.RaiseAndSetIfChanged(ref _statusLabelColor, value);
        }

        private FontWeight _statusLabelFontWeight;
        public FontWeight StatusLabelFontWeight
        {
            get => _statusLabelFontWeight;
            set => this.RaiseAndSetIfChanged(ref _statusLabelFontWeight, value);
        }

        private InstallationState _state;
        public InstallationState State
        {
            get => _state;
            private set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        private string _statusLabel;
        public string StatusLabel
        {
            get => _statusLabel;
            private set => this.RaiseAndSetIfChanged(ref _statusLabel, value);
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            private set => this.RaiseAndSetIfChanged(ref _isIndeterminate, value);
        }

        private long _totalProgress;
        public long TotalProgress
        {
            get => _totalProgress;
            private set => this.RaiseAndSetIfChanged(ref _totalProgress, value);
        }

        private long _currentProgress;
        public long CurrentProgress
        {
            get => _currentProgress;
            private set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
        }

        public InstallingPackageViewModel(PackageManifest manifest)
        {
            _manifest = manifest;
            _installationTask = null;
            _isIndeterminate = true;
            _statusLabel = "Inactive";
            _totalProgress = 0;
            _currentProgress = 0;
            _state = InstallationState.Pending;
            _statusLabelColor = Brushes.Black;
            _statusLabelFontWeight = FontWeight.Normal;
        }

        void IProgressMonitor.RequestPending(string packageId)
        {
            if (packageId.Equals(Id))
            {
                StatusLabel = "Requesting data...";
                IsIndeterminate = true;
            }
        }

        void IProgressMonitor.DownloadStarted(IDownloadProgressMonitor monitor)
        {
            if (monitor.PackageId.Equals(Id))
            {
                StatusLabel = "Downloading ...";
                IsIndeterminate = monitor.IsIndeterminate;
                CurrentProgress = 0;
                TotalProgress = monitor.TotalSize;
                monitor.DownloadProgress += OnDownloadProgress;
            }
        }

        private void OnDownloadProgress(IDownloadProgressMonitor downloadProgressMonitor)
        {
            Debug.Assert(downloadProgressMonitor.PackageId.Equals(Id));
            CurrentProgress = downloadProgressMonitor.CurrentSize;
            TotalProgress = downloadProgressMonitor.TotalSize;
        }

        void IProgressMonitor.ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            if (packageId.Equals(Id))
            {
                StatusLabel = "Extracting...";
                IsIndeterminate = true;
            }
        }

        void IProgressMonitor.ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            if (packageId.Equals(Id))
            {
                StatusLabel = "Extraction completed!";
                IsIndeterminate = false;
                CurrentProgress = 100;
                TotalProgress = 100;
            }
        }

        void IProgressMonitor.CopyingStarted(string packageId, IVersionNumber versionNumber)
        {
            if (packageId.Equals(Id))
            {
                StatusLabel = "Copying...";
                IsIndeterminate = true;
            }
        }

        void IProgressMonitor.CopyingCompleted(string packageId, IVersionNumber versionNumber)
        {
            if (packageId.Equals(Id))
            {
                StatusLabel = "Copying completed!";
                IsIndeterminate = false;
                CurrentProgress = 100;
                TotalProgress = 100;
            }
        }
    }
}
