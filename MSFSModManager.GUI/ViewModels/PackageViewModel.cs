// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using MSFSModManager.Core;
using System.Threading.Tasks;
using ReactiveUI;
using System.Linq;

namespace MSFSModManager.GUI.ViewModels
{
    class PackageViewModel : ViewModelBase
    {

        private InstalledPackage _package;

        public bool IsInstalled => (_package.Manifest != null);

        public bool ShouldBeUpdated { get; set; }

        public string Id => _package.Id;

        public string Type => _package.Type;

        public string Version => (_package.Version != null) ? _package.Version.ToString() : "";

        private string _latestVersion;
        public string LatestVersion
        {
            get => _latestVersion;
            set => this.RaiseAndSetIfChanged(ref _latestVersion, value);
        }

        public string Creator => (_package.Manifest != null) ? ((_package.Manifest.Creator != null) ? _package.Manifest.Creator : "") : "";

        public string Source => (_package.PackageSource != null) ? _package.PackageSource.ToString()! : "";

        public PackageViewModel(InstalledPackage package)
        {
            _package = package;
            ShouldBeUpdated = false;
            _latestVersion = "";

            FetchLatestVersion().ContinueWith(
                v => _latestVersion = (v.Result != null) ? v.Result.ToString()! : ""
            );
        }

        private async Task<IVersionNumber?> FetchLatestVersion()
        {
            if (_package.PackageSource != null)
                return (await _package.PackageSource!.ListAvailableVersions()).Max();
            return null;
        }
    }
}