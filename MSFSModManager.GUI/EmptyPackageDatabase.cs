// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.GUI
{
    class EmptyPackageDatabase : IPackageDatabase
    {
        public IEnumerable<InstalledPackage> Packages => Enumerable.Empty<InstalledPackage>();

        public IEnumerable<InstalledPackage> CommunityPackages => Enumerable.Empty<InstalledPackage>();

        public IEnumerable<InstalledPackage> OfficialPackages => Enumerable.Empty<InstalledPackage>();

        public void AddPackageSource(string packageId, IPackageSource packageSource)
        {
            throw new NotSupportedException();
        }

        public bool Contains(string packageId) => false;

        public bool Contains(string packageId, VersionBounds versionBounds) => false;

        public InstalledPackage GetInstalledPackage(string packageId)
        {
            throw new PackageNotInstalledException(packageId);
        }

        public Task InstallPackage(IPackageInstaller installer, IProgressMonitor? monitor = null, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public void RemovePackageSource(string packageId)
        {
        }

        public void Uninstall(string packageId)
        {
        }
    }
}
