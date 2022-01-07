// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{

    /// <summary>
    /// A repository that hosts several package sources.
    /// </summary>
    public interface IPackageSourceRepository
    {
        Task<IEnumerable<string>> ListAvailablePackages();

        IPackageSource GetSource(string packageId);

        bool HasSource(string packageId);
    }
}
