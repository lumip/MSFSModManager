// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading.Tasks;
using System.Net.Http;

namespace MSFSModManager.Core.PackageSources
{
    public interface IPackageInstaller
    {
        string PackageId { get; }

        Task Install(string destination, IProgressMonitor? monitor = null);
    }

}
