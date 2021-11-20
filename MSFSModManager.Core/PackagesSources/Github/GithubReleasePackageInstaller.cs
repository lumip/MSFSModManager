// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading.Tasks;
using System.IO;

namespace MSFSModManager.Core.PackageSources.Github
{
    public class GithubReleasePackageInstaller : IPackageInstaller
    {

        private GithubArtifactDownloader _downloader;

        internal GithubReleasePackageInstaller(
            GithubArtifactDownloader downloader
        )
        {
            _downloader = downloader;
        }

        public string PackageId => _downloader.PackageId;

        private IVersionNumber Version => _downloader.Version;
        private string CacheId => _downloader.CacheId;

        private PackageCache Cache => _downloader.Cache;

        public async Task Install(string destination, IProgressMonitor? monitor)
        {
            string packageFolder;

            if (Cache.Contains(_downloader.CacheId, _downloader.Version))
            {
                packageFolder = Cache.GetPath(CacheId, Version);
            }
            else
            {
                packageFolder = await _downloader.DownloadToCache(monitor);
            }

            string manifestFilePath = GithubReleasePackageSource.LocateManifest(packageFolder);
            string manifestFolderPath = Path.GetDirectoryName(manifestFilePath);
            await new CachedPackageInstaller(PackageId, manifestFolderPath).Install(destination, monitor);
        }

    }
}
