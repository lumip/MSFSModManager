// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using ReactiveUI;

namespace MSFSModManager.GUI.ViewModels
{

    /// <summary>
    /// Used as placeholder instead of PackageViewModel in PackageDetailsView before the user selects a package to display.
    /// </summary>
    class NullPackageViewModel : ViewModelBase
    {
        public bool IsInstalled => false;

        public bool IsCommunityPackage => true;

        private bool _markedForInstall;
        public bool MarkedForInstall
        {
            get => _markedForInstall;
            set => this.RaiseAndSetIfChanged(ref _markedForInstall, value);
        }

        public string Id => "<package id>";

        public string Type => "<package type>";

        public string Version => "not installed";

        public string LatestVersionString => "<latest version>";

        public string Creator => "<package creator>";

        public string Source => "<package source>";
        public bool HasSource => false;

        public string Title => "<package title>";

        public bool IsLatestVersionNewer => false;

        public string AddSourceLabel => "Add Source";

        public string MarkForInstallLabel => "Select for Installation";

        
        public IReactiveCommand? OpenAddPackageDialogCommand => null;
        public IReactiveCommand? RemovePackageSourceCommand => null;

        public IReactiveCommand? UninstallPackageCommand => null;

        public NullPackageViewModel()
        {
            _markedForInstall = false;
        }

    }
}
