// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MSFSModManager.Core.PackageSources
{
    public class PackageDatabaseSource : IPackageSourceRepository
    {

        IPackageDatabase _database;

        public PackageDatabaseSource(IPackageDatabase database)
        {
            _database = database;
        }

        public Task<IEnumerable<string>> ListAvailablePackages(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(
                _database.CommunityPackages.Where(p => p.PackageSource != null).Select(p => p.Id)
            );
        }

        public IPackageSource GetSource(string packageId)
        {
            try
            {
                InstalledPackage package = _database.GetInstalledPackage(packageId);
                if (package.PackageSource == null)
                    return new LocallyInstalledPackageSource(package);
                IPackageSource source = package.PackageSource;
                return source;
            }
            catch (KeyNotFoundException)
            {
                throw new PackageNotAvailableException(packageId);
            }
        }

        public bool HasSource(string packageId)
        {
            return _database.Contains(packageId);
        }
    }
}