// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.Threading;
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

        public async Task<PackageManifest> Install(string destination, IProgressMonitor? monitor, CancellationToken cancellationToken)
        {
            string packageFolder;

            if (Cache.Contains(_downloader.CacheId, _downloader.Version))
            {
                packageFolder = Cache.GetPath(CacheId, Version);
            }
            else
            {
                packageFolder = await _downloader.DownloadToCache(monitor, cancellationToken);
            }

            string manifestFilePath = GithubReleasePackageSource.LocateManifest(packageFolder);
            string manifestFolderPath = Path.GetDirectoryName(manifestFilePath);

            PackageManifest manifest = PackageManifest.FromFile(PackageId, manifestFilePath);
            return await new CachedPackageInstaller(manifest, manifestFolderPath).Install(destination, monitor, cancellationToken);
        }

    }
}
