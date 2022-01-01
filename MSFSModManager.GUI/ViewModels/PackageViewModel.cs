// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using MSFSModManager.Core;
using System.Threading.Tasks;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using System;

using MSFSModManager.Core.PackageSources;
using System.Windows.Input;
using System.Reactive;

namespace MSFSModManager.GUI.ViewModels
{
    class PackageViewModel : ViewModelBase
    {

        private InstalledPackage _package;

        public bool IsInstalled => (_package.Manifest != null);

        private bool _markedForInstall;
        public bool MarkedForInstall
        {
            get => _markedForInstall;
            set => this.RaiseAndSetIfChanged(ref _markedForInstall, value);
        }

        public string Id => _package.Id;

        public string Type => _package.Type;

        public string Version => _package.Version?.ToString() ?? "not installed";

        private IVersionNumber? _latestVersion;
        private IVersionNumber? LatestVersion
        {
            get => _latestVersion;
            set => this.RaiseAndSetIfChanged(ref _latestVersion, value);
        }

        private ObservableAsPropertyHelper<string> _latestVersionString;
        public string LatestVersionString => _latestVersionString.Value;

        public string Creator => _package.Manifest?.Creator ?? "N/A";

        public string Source => _package.PackageSource?.ToString() ?? "";
        public bool HasSource => _package.PackageSource != null;

        public string Title => _package.Manifest?.Title ?? "N/A";

        private readonly ObservableAsPropertyHelper<bool> _isLatestVersionNewer;
        public bool IsLatestVersionNewer => _isLatestVersionNewer.Value;

        
        public ICommand OpenAddPackageDialogCommand { get; }
        public ICommand RemovePackageSourceCommand { get; }

        public PackageViewModel(InstalledPackage package, ICommand openAddPackageDialogCommand, ReactiveCommand<InstalledPackage, Unit> removePackageSourceCommand, PackageVersionCache versionCache)
        {
            _package = package;
            _markedForInstall = false;
            _latestVersion = null;

            _isLatestVersionNewer = this
                .WhenAnyValue(x => x.LatestVersion)
                .Select(v => v != null && (_package.Manifest == null || v > _package.Manifest.Version))
                .ToProperty(this, x => x.IsLatestVersionNewer, out _isLatestVersionNewer);
            _latestVersionString = this
                .WhenAnyValue(x => x.LatestVersion)
                .Select(v => v?.ToString() ?? "")
                .ToProperty(this, x => x.LatestVersionString, out _latestVersionString);

            FetchLatestVersion(versionCache).ContinueWith(
                v => LatestVersion = v.Result,
                TaskContinuationOptions.OnlyOnRanToCompletion
            );

            OpenAddPackageDialogCommand = openAddPackageDialogCommand;
            RemovePackageSourceCommand = ReactiveCommand.Create(() => {
                removePackageSourceCommand.Execute(_package).Wait();
            });
        }

        private async Task<IVersionNumber?> FetchLatestVersion(PackageVersionCache versionCache)
        {
            if (_package.PackageSource != null)
            {
                if (versionCache.HasVersion(_package))
                    return versionCache.GetCachedVersion(_package);

                IVersionNumber? version = (await _package.PackageSource.ListAvailableVersions()).Max();
                if (version != null) versionCache.UpdateCachedVersion(_package, version);

                return version;
            }
            return null;
        }
    }
}