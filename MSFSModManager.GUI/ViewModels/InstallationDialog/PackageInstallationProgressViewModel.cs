using Avalonia;
using ReactiveUI;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using System.Diagnostics;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using System;

namespace MSFSModManager.GUI.ViewModels
{
    class PackageInstallationProgressViewModel : ViewModelBase, IProgressMonitor
    {

        private SourceCache<InstallingPackageViewModel, string> _packagesToInstall;
        private Dictionary<string, InstallingPackageViewModel> _packageLookup;

        private IDisposable _dynamicData;
        private ReadOnlyObservableCollection<InstallingPackageViewModel> _installingPackages;
        public ReadOnlyObservableCollection<InstallingPackageViewModel> InstallingPackages => _installingPackages;

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => this.RaiseAndSetIfChanged(ref _isProgressVisible, value);
        }

        public PackageInstallationProgressViewModel(IEnumerable<PackageManifest> packagesToInstall)
        {
            _isProgressVisible = true;
            
            _packagesToInstall = new SourceCache<InstallingPackageViewModel, string>(p => p.Id);
            _packagesToInstall.AddOrUpdate(packagesToInstall.Select(m => new InstallingPackageViewModel(m)));

            _packageLookup = new Dictionary<string, InstallingPackageViewModel>(
                _packagesToInstall.KeyValues
            );

            _dynamicData = _packagesToInstall
                                        .Connect()
                                        .ObserveOn(RxApp.MainThreadScheduler)
                                        .Bind(out _installingPackages)
                                        .Subscribe();

        }

        public void RequestPending(string packageId)
        {
            if (_packageLookup.ContainsKey(packageId))
                ((IProgressMonitor)_packageLookup[packageId]).RequestPending(packageId);
        }

        public void DownloadStarted(IDownloadProgressMonitor monitor)
        {
            if (_packageLookup.ContainsKey(monitor.PackageId))
                ((IProgressMonitor)_packageLookup[monitor.PackageId]).DownloadStarted(monitor);
        }

        public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
        {
            if (_packageLookup.ContainsKey(packageId))
                ((IProgressMonitor)_packageLookup[packageId]).ExtractionStarted(packageId, versionNumber);
        }

        public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
        {
            if (_packageLookup.ContainsKey(packageId))
                ((IProgressMonitor)_packageLookup[packageId]).ExtractionCompleted(packageId, versionNumber);
        }

        public void SetInstallationTask(string packageId, Task installationTask)
        {
            if (_packageLookup.ContainsKey(packageId))
            {
                _packageLookup[packageId].InstallationTask = installationTask;
            }
        }

        public void CopyingStarted(string packageId, IVersionNumber versionNumber)
        {
            if (_packageLookup.ContainsKey(packageId))
                ((IProgressMonitor)_packageLookup[packageId]).CopyingStarted(packageId, versionNumber);
        }

        public void CopyingCompleted(string packageId, IVersionNumber versionNumber)
        {
            if (_packageLookup.ContainsKey(packageId))
                ((IProgressMonitor)_packageLookup[packageId]).CopyingCompleted(packageId, versionNumber);
        }
    }
}
