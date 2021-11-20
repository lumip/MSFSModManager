// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    public class HiddenBasePackageSource : AbstractPackageSource
    {
        private string _packageId;

        public HiddenBasePackageSource(string packageId)
        {
            _packageId = packageId;
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            throw new NotSupportedException("Installation of base packages not supported.");
        }

        public override Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor)
        {
            VersionNumber? versionNumber = versionBounds.Lower as VersionNumber;
            if (versionNumber == null)
            {
                versionNumber = VersionNumber.Zero;
            }
            PackageManifest manifest = new PackageManifest(
                _packageId, _packageId, versionNumber, VersionNumber.Zero, "BASE", new PackageDependency[0], "asobo"
            );
            return Task.FromResult(manifest);
        }

        public override Task<IEnumerable<IVersionNumber>> ListAvailableVersions()
        {
            throw new NotSupportedException();
        }

        public override JToken Serialize()
        {
            throw new NotImplementedException();
        }
    }

}