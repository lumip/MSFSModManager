// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using MSFSModManager.Core;

namespace MSFSModManager.GUI
{
    public class PackageVersionCache
    {
        private Dictionary<string, IVersionNumber> _cachedVersions;

        public PackageVersionCache()
        {
            _cachedVersions = new Dictionary<string, IVersionNumber>();
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
            _cachedVersions.Add(packageId, version);
        }

        public void UpdateCachedVersion(InstalledPackage package, IVersionNumber version)
        {
            UpdateCachedVersion(package.Id, version);
        }
    }
}
