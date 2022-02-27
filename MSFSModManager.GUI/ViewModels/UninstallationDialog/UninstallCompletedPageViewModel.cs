// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System.Collections.Generic;

namespace MSFSModManager.GUI.ViewModels
{
    class UninstallCompletedPageViewModel : ViewModelBase
    {

        public IEnumerable<UninstallingPackageViewModel> PackagesUninstalled { get; private set; }
        public IEnumerable<UninstallingPackageViewModel> PackagesFailed { get; private set; }

        public UninstallCompletedPageViewModel(
            IEnumerable<UninstallingPackageViewModel> succeededUninstallations,
            IEnumerable<UninstallingPackageViewModel> failedUninstallations)
        {
            PackagesUninstalled = succeededUninstallations;
            PackagesFailed = failedUninstallations;
        }
    }
}
