using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;

namespace MSFSModManager.Core.PackageSources.Github
{
    public class GithubReleasePackageInstaller : IPackageInstaller
    {

        private GithubReleaseDownloader _downloader;

        internal GithubReleasePackageInstaller(
            GithubReleaseDownloader downloader
        )
        {
            _downloader = downloader;
        }

        public string PackageId => _downloader.PackageId;

        private VersionNumber Version => _downloader.Version;
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
