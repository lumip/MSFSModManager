// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021-2022 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MSFSModManager.Core.PackageSources
{

    class LocallyInstalledPackageSource : AbstractPackageSource
    {

        private InstalledPackage _installedPackage;

        public LocallyInstalledPackageSource(InstalledPackage installedPackage)
        {
            _installedPackage = installedPackage;
            if (_installedPackage.Manifest == null)
                throw new ArgumentException("Installed package must have a manifest.", nameof(installedPackage));
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            var versionBounds = new VersionBounds(versionNumber);
            if (_installedPackage.Version == null || !versionBounds.CheckVersion(_installedPackage.Version))
                throw new VersionNotAvailableException(_installedPackage.Id, versionBounds);
            return new NoOpPackageInstaller(_installedPackage.Manifest!);
        }

        public override Task<PackageManifest> GetPackageManifest(
            VersionBounds versionBounds,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            PackageManifest manifest = _installedPackage.Manifest!;
            if (versionBounds.CheckVersion(manifest.Version))
            {
                return Task.FromResult(manifest);
            }
            throw new VersionNotAvailableException(_installedPackage.Id, versionBounds);
        }

        public override Task<IEnumerable<IVersionNumber>> ListAvailableVersions(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IVersionNumber[] versions = new VersionNumber[] { _installedPackage.Manifest!.Version };
            return Task.FromResult(versions.AsEnumerable());
        }

        public override JToken Serialize()
        {
            throw new System.NotImplementedException();
        }

        public override string AsSourceString()
        {
            throw new NotImplementedException();
        }

        public override string PackageId => _installedPackage.Id;
    }

}