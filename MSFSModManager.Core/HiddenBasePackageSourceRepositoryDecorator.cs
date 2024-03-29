// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{

    /// <summary>
    /// A decorator for a <see cref="IPackageSourceRepository" /> instance that provides
    /// dummy sources for hidden "fs-base" packages.
    /// 
    /// These packages are all built-in system packages that are installed/updated
    /// by the game itself and not present in the official packages folder. They cannot
    /// be obtained via the default package source mechanisms and are invisible to <see cref="PackageDatabase" />.
    /// This decorator allows treating them in the default way nevertheless to allow
    /// package dependency resolution to work.
    /// </summary>
    public class HiddenBasePackageSourceRepositoryDecorator : IPackageSourceRepository
    {

        IPackageSourceRepository _repository;

        public HiddenBasePackageSourceRepositoryDecorator(IPackageSourceRepository repository)
        {
            _repository = repository;
        }

        public IPackageSource GetSource(string packageId)
        {
            try
            {
                // If our base repository can get us a package source, we want to return it in any case.
                return _repository.GetSource(packageId);
            }
            catch (PackageNotAvailableException)
            {
                // There as some "fs-base" packages which are hidden, i.e., they are not in the database.
                // We just have to assume they are there and return a HiddenBasePackageSource in this case.
                return new HiddenBasePackageSource(packageId);
            }
        }

        public bool HasSource(string packageId)
        {
            if (packageId.StartsWith("fs-base")) return true;
            return _repository.HasSource(packageId);
        }

        public Task<IEnumerable<string>> ListAvailablePackages(
            CancellationToken cancellationToken = default(CancellationToken)
        )
        {
            return _repository.ListAvailablePackages(cancellationToken);
        }
        
    }
}
