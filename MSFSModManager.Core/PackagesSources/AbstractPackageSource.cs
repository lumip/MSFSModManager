// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources
{
    public abstract class AbstractPackageSource : IPackageSource
    {
        public virtual Task<PackageManifest> GetPackageManifest(
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken)) => GetPackageManifest(VersionBounds.Unbounded, gameVersion, monitor, cancellationToken);
        public abstract Task<PackageManifest> GetPackageManifest(
            VersionBounds versionBounds,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract IPackageInstaller GetInstaller(IVersionNumber versionNumber);

        public abstract JToken Serialize();

        public abstract Task<IEnumerable<IVersionNumber>> ListAvailableVersions(
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract string AsSourceString();

        public abstract string PackageId { get; }
    }
}