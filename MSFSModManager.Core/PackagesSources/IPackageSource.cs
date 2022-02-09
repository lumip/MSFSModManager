// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System; 
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{
        
    public interface IPackageSource : IJsonSerializable
    {
        Task<PackageManifest> GetPackageManifest(
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken));
            
        Task<PackageManifest> GetPackageManifest(
            VersionBounds versionBounds,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken));

        IPackageInstaller GetInstaller(IVersionNumber versionNumber);

        Task<IEnumerable<IVersionNumber>> ListAvailableVersions(
            CancellationToken cancellationToken = default(CancellationToken));

        string AsSourceString();

        string PackageId { get; }

    }

}