// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO.Compression;

using MSFSModManager.Core.Parsing;

namespace MSFSModManager.Core.PackageSources
{

    public class ZipFilePackageSource : AbstractPackageSource
    {

        private string _filePath;
        public override string PackageId => _manifest.Id;
        private PackageManifest _manifest;

        public ZipFilePackageSource(string packageId, string filePath)
        {
            _filePath = filePath;

            using (ZipPackageArchive archive = new ZipPackageArchive(filePath))
            {
                using (Stream manifestStream = archive.OpenManifest())
                {
                    _manifest = PackageManifest.FromStream(packageId, manifestStream);
                }
            }
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            return new ZipFilePackageInstaller(_manifest, _filePath);
        }

        public override Task<PackageManifest> GetPackageManifest(
            VersionBounds versionBounds,
            IVersionNumber gameVersion,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            IVersionNumber packageVersion = _manifest.SourceVersion;
            if (!versionBounds.CheckVersion(packageVersion))
                throw new VersionNotAvailableException(PackageId, versionBounds);
            
            return Task.FromResult(_manifest);
        }

        public override Task<IEnumerable<IVersionNumber>> ListAvailableVersions(
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            IVersionNumber[] versions = new VersionNumber[] { _manifest.Version };
            return Task.FromResult(versions.AsEnumerable());
        }

        public override JToken Serialize()
        {
            return _filePath;
        }

        public static ZipFilePackageSource Deserialize(string packageId, JToken serialized)
        {
            string filePath = JsonUtils.Cast<string>(serialized);
            return new ZipFilePackageSource(packageId, filePath);
        }

        public override string ToString() => AsSourceString();

        public override string AsSourceString()
        {
            return _filePath;
        }
    }
}
