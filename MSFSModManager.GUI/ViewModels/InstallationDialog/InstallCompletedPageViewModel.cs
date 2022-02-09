// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System.Collections.Generic;

namespace MSFSModManager.GUI.ViewModels
{
    class InstallCompletedPageViewModel : ViewModelBase
    {

        public IEnumerable<InstallingPackageViewModel> PackagesInstalled { get; private set; }
        public IEnumerable<InstallingPackageViewModel> PackagesFailed { get; private set; }

        public InstallCompletedPageViewModel(
            IEnumerable<InstallingPackageViewModel> succeededInstallations,
            IEnumerable<InstallingPackageViewModel> failedInstallations)
        {
            PackagesInstalled = succeededInstallations;
            PackagesFailed = failedInstallations;
        }
    }
}
