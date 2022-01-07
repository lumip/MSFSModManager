// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using MSFSModManager.Core;

namespace MSFSModManager.GUI
{
    public class PackageVersionCache
    {
        private ConcurrentDictionary<string, IVersionNumber> _cachedVersions;

        public PackageVersionCache()
        {
            _cachedVersions = new ConcurrentDictionary<string, IVersionNumber>();
        }

        public bool HasVersion(string packageId)
        {
            return _cachedVersions.ContainsKey(packageId);
        }

        public bool HasVersion(InstalledPackage package)
        {
            return HasVersion(package.Id);
        }

        public IVersionNumber GetCachedVersion(string packageId)
        {
            return _cachedVersions[packageId];
        }

        public IVersionNumber GetCachedVersion(InstalledPackage package)
        {
            return GetCachedVersion(package.Id);
        }

        public void UpdateCachedVersion(string packageId, IVersionNumber version)
        {
            _cachedVersions.AddOrUpdate(packageId, _ => version, (id, v) => version);
        }

        public void UpdateCachedVersion(InstalledPackage package, IVersionNumber version)
        {
            UpdateCachedVersion(package.Id, version);
        }

        /// <summary>
        /// Fetches the latest available version number for the package.
        /// 
        /// If a version number was already cached, the cached value is returned. Otherwise
        /// a lookup from the package source of the given package will be performed and returned
        /// value is added to the cache.
        /// </summary>
        public async Task<IVersionNumber?> FetchAvailableVersionNumber(InstalledPackage package)
        {
            if (package.PackageSource != null)
            {
                if (HasVersion(package))
                    return GetCachedVersion(package);

                IVersionNumber? version = (await package.PackageSource.ListAvailableVersions()).Max();
                if (version != null) UpdateCachedVersion(package, version);

                return version;
            }
            return null;
        }
    }
}
