// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Collections.Generic;
using System.Threading.Tasks;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    /// <summary>
    /// Database of locally installed and registered packages for MSFS Flight Simulator.
    /// 
    /// This is the in-program representation of the MSFS data folder.
    /// </summary>
    public interface IPackageDatabase
    {
        IEnumerable<InstalledPackage> Packages { get; }
        IEnumerable<InstalledPackage> CommunityPackages { get; }
        IEnumerable<InstalledPackage> OfficialPackages { get; }
        bool Contains(string packageId);
        bool Contains(string packageId, VersionBounds versionBounds);
        InstalledPackage GetInstalledPackage(string packageId);

        Task InstallPackage(IPackageInstaller installer, IProgressMonitor? monitor = null);
        void Uninstall(string packageId);

        void AddPackageSource(string packageId, IPackageSource packageSource);
        void RemovePackageSource(string packageId);
    }
}