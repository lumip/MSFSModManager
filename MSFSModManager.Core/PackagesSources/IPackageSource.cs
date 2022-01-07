// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{
        
    public interface IPackageSource : IJsonSerializable
    {
        Task<PackageManifest> GetPackageManifest(IVersionNumber gameVersion, IProgressMonitor? monitor = null);
        Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor = null);

        IPackageInstaller GetInstaller(IVersionNumber versionNumber);

        Task<IEnumerable<IVersionNumber>> ListAvailableVersions();

        string AsSourceString();

        string PackageId { get; }

    }

}