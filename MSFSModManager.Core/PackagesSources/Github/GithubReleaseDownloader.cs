using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.IO.Compression;

namespace MSFSModManager.Core.PackageSources.Github
{
    class GithubReleaseDownloader
    {

        public string PackageId { get; }

        public VersionNumber Version { get; }
        private string _url;
        private HttpClient _client;

        private GithubRepository _repository;

        public string CacheId => $"{_repository.Organisation}_{_repository.Name}";

        internal PackageCache Cache { get; }

        public GithubReleaseDownloader(
            string packageId,
            VersionNumber version,
            GithubRepository repository,
            string url,
            HttpClient httpClient,
            PackageCache cache
        )
        {
            PackageId = packageId;
            Version = version;
            _url = url;
            _client = httpClient;
            Cache = cache;
            _repository = repository;
        }

        public async Task<string> DownloadToFile(IProgressMonitor? monitor = null)
        {

            string archiveFilePath = Path.GetTempFileName();

            try
            {
                HttpResponseMessage response = await _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);

                long totalBytes = Convert.ToInt64(response.Content.Headers.GetValues("Content-Length").First());
                HttpClientDownloadProgressMonitor downloadMonitor = new HttpClientDownloadProgressMonitor(PackageId, Version, totalBytes);

                monitor?.DownloadStarted(downloadMonitor);


                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = File.Create(archiveFilePath))
                {
                    await responseStream.CopyToAsync(fileStream, downloadMonitor, CancellationToken.None);
                }
                return archiveFilePath;
            }
            catch (Exception e)
            {
                File.Delete(archiveFilePath);
                throw e;
            }
        }

        public async Task<string> DownloadToCache(IProgressMonitor? monitor = null)
        {
            string archiveFilePath = await DownloadToFile(monitor);
            try
            {
                string packageFolder = Cache.AddCacheEntry(CacheId, Version);
                
                monitor?.ExtractionStarted(PackageId, Version);
                ZipFile.ExtractToDirectory(archiveFilePath, packageFolder);
                monitor?.ExtractionCompleted(PackageId, Version);
                return packageFolder;
            }
            catch (Exception e)
            {
                Cache.RemoveCacheEntry(CacheId, Version);
                throw e;
            }
            finally
            {
                File.Delete(archiveFilePath);
            }
        }

    }
}