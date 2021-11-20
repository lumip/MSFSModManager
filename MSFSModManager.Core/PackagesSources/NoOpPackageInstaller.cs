// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Net.Http;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{
    class NoOpPackageInstaller : IPackageInstaller
    {
        public string PackageId { get; }

        public NoOpPackageInstaller(string packageId)
        {
            PackageId = packageId;
        }

        public Task Install(string destination, IProgressMonitor? monitor)
        {
            return Task.CompletedTask;
        }

    }
}
