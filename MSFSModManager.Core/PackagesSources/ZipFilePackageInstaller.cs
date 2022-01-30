// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{

    public class ZipFilePackageInstaller : IPackageInstaller
    {
        private string _pathToArchive;
        public string PackageId => _manifest.Id;
        private IVersionNumber VersionNumber => _manifest.SourceVersion;
        private PackageManifest _manifest;

        public ZipFilePackageInstaller(PackageManifest manifest, string pathToArchive)
        {
            _pathToArchive = pathToArchive;
            _manifest = manifest;
        }

        public async Task<PackageManifest> Install(
            string destination, IProgressMonitor? monitor, CancellationToken cancellationToken
        )
        {
            monitor?.ExtractionStarted(PackageId, VersionNumber);
            using (ZipPackageArchive archive = new ZipPackageArchive(_pathToArchive))
            {
                await Task.Run(() => archive.Extract(destination), cancellationToken);
            }
            monitor?.ExtractionCompleted(PackageId, VersionNumber);
            return _manifest;
        }
    }

}
