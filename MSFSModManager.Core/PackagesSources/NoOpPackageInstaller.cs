// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{
    class NoOpPackageInstaller : IPackageInstaller
    {
        private PackageManifest _manifest;

        public string PackageId => _manifest.Id;

        public NoOpPackageInstaller(PackageManifest manifest)
        {
            _manifest = manifest;
        }

        public Task<PackageManifest> Install(string destination, IProgressMonitor? monitor, CancellationToken cancellationToken)
        {
            return Task.FromResult(_manifest);
        }

    }
}
