// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    /// <summary>
    /// A package source for "fs-base" packages.
    /// 
    /// These packages are all built-in system packages that are installed/updated
    /// by the game itself and cannot be obtained via the default package source mechanisms.
    /// This class allows treating them in the default way during dependency resolution
    /// by providing dummy manifests.
    /// 
    /// <see cref="IPackageSource.GetInstaller(IVersionNumber)" /> and <see cref="IPackageSource.ListAvailableVersions" />
    /// are not available.
    /// </summary>
    public class HiddenBasePackageSource : AbstractPackageSource
    {
        public override string PackageId { get; }

        public HiddenBasePackageSource(string packageId)
        {
            PackageId = packageId;
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            throw new NotSupportedException("Installation of base packages not supported.");
        }

        public override Task<PackageManifest> GetPackageManifest(
            VersionBounds versionBounds,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            VersionNumber? versionNumber = versionBounds.Lower as VersionNumber;
            if (versionNumber == null)
            {
                versionNumber = VersionNumber.Zero;
            }
            PackageManifest manifest = new PackageManifest(
                PackageId, PackageId, versionNumber, VersionNumber.Zero, "BASE", new PackageDependency[0], "asobo"
            );
            return Task.FromResult(manifest);
        }

        public override Task<IEnumerable<IVersionNumber>> ListAvailableVersions(
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            throw new NotSupportedException();
        }

        public override JToken Serialize()
        {
            throw new NotSupportedException();
        }

        public override string AsSourceString()
        {
            throw new NotSupportedException();
        }

    }

}