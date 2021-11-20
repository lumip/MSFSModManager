// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;

namespace MSFSModManager.Core
{

    public class PackageCache
    {

        private struct PackageCacheKey
        {
            public string PackageId { get; }
            public string Version { get; }

            public PackageCacheKey(string packageId, string versionNumber)
            {
                PackageId = packageId;
                Version = versionNumber;
            }

            public PackageCacheKey(string packageId, IVersionNumber versionNumber)
                : this(packageId, versionNumber.ToString()) { }
        }

        private Dictionary<PackageCacheKey, string> _cachedPackagePaths;
        private string _cachePath;
        private int _cacheSize;

        public PackageCache(string cachePath, int cacheSize = -1)
        {
            _cachePath = cachePath;
            Directory.CreateDirectory(_cachePath);
            _cacheSize = cacheSize;

            _cachedPackagePaths = new Dictionary<PackageCacheKey, string>();

            DirectoryInfo cacheDir = new DirectoryInfo(_cachePath);
            foreach (DirectoryInfo packageDir in cacheDir.GetDirectories())
            {
                foreach (DirectoryInfo versionDir in packageDir.GetDirectories())
                {
                    PackageCacheKey key = new PackageCacheKey(packageDir.Name, versionDir.Name);
                    _cachedPackagePaths.Add(key, Path.GetRelativePath(_cachePath, versionDir.FullName));
                }
            }
        }

        public bool Contains(string packageId, IVersionNumber versionNumber)
        {
            PackageCacheKey cacheKey = new PackageCacheKey(packageId, versionNumber);
            return _cachedPackagePaths.ContainsKey(cacheKey);
        }

        public string GetPath(string packageId, IVersionNumber versionNumber)
        {
            PackageCacheKey cacheKey = new PackageCacheKey(packageId, versionNumber);
            string relativePath = _cachedPackagePaths[cacheKey];
            string path = Path.Join(_cachePath, relativePath);
            return path;
        }

        public string AddCacheEntry(string packageId, IVersionNumber versionNumber)
        {
            string path;
            PackageCacheKey cacheKey = new PackageCacheKey(packageId, versionNumber);
            if (_cachedPackagePaths.ContainsKey(cacheKey))
            {
                string relativePath = _cachedPackagePaths[cacheKey];
                path = Path.Join(_cachePath, relativePath);
            }
            else
            {

                string relativePath = Path.Join(packageId, versionNumber.ToString());
                _cachedPackagePaths.Add(cacheKey, relativePath);
                path = Path.Join(_cachePath, relativePath);

                Directory.CreateDirectory(path);
            }
            return path;
        }

        public void RemoveCacheEntry(string packageId, IVersionNumber versionNumber)
        {
            PackageCacheKey cacheKey = new PackageCacheKey(packageId, versionNumber);
            if (_cachedPackagePaths.ContainsKey(cacheKey))
            {
                string relativePath = _cachedPackagePaths[cacheKey];
                
                string path = $"{_cachePath}/{relativePath}";
                Directory.Delete(path, recursive: true);
                path = Path.GetDirectoryName(path);
                while (path.StartsWith(_cachePath) && path != _cachePath)
                {
                    try
                    {
                        Directory.Delete(path, recursive: false);
                        path = Path.GetDirectoryName(path);
                    }
                    catch (Exception e)
                    {
                        GlobalLogger.Log(LogLevel.Error, $"Could not delete cache directory {path}:\n{e}");
                    }
                }

                _cachedPackagePaths.Remove(cacheKey);
            }
        }
    }
}