// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

namespace MSFSModManager.Core.PackageSources
{
    public interface IPackageInstaller
    {
        string PackageId { get; }

        Task<PackageManifest> Install(
            string destination,
            IProgressMonitor? monitor = null,
            CancellationToken cancellationToken = default(CancellationToken)
        );
    }

}
