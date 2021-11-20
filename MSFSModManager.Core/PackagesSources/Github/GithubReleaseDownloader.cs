using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.IO.Compression;

namespace MSFSModManager.Core.PackageSources.Github
{
    class GithubReleaseDownloader
    {

        public string PackageId { get; }

        public IVersionNumber Version { get; }
        private string _url;
        private HttpClient _client;

        private GithubRepository _repository;

        public string CacheId => $"{_repository.Organisation}_{_repository.Name}";

        internal PackageCache Cache { get; }

        public GithubReleaseDownloader(
            string packageId,
            IVersionNumber version,
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("lumip", "0.1"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                using (HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead))
                {

                    // HttpResponseMessage response = await _client.GetAsync(_url, HttpCompletionOption.ResponseHeadersRead);

                    GlobalLogger.Log(LogLevel.Warning, $"{response.StatusCode}");
                    GlobalLogger.Log(LogLevel.Warning, "################################");
                    GlobalLogger.Log(LogLevel.Warning, response.Headers.ToString());
                    GlobalLogger.Log(LogLevel.Warning, "################################");
                    GlobalLogger.Log(LogLevel.Warning, response.Content.Headers.ToString());

                    response.EnsureSuccessStatusCode();

                    IDownloadProgressMonitor downloadMonitor;

                    try
                    {
                        long totalBytes = Convert.ToInt64(response.Content.Headers.GetValues("Content-Length").First());
                        downloadMonitor = new HttpClientDownloadProgressMonitor(PackageId, Version, totalBytes);
                    }
                    catch (InvalidOperationException)
                    {
                        downloadMonitor = new HttpClientUnknownSizeDownloadProgressMonitor(PackageId, Version);
                    }

                    monitor?.DownloadStarted(downloadMonitor);


                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(archiveFilePath))
                    {
                        await responseStream.CopyToAsync(fileStream, (IProgress<long>)downloadMonitor, CancellationToken.None);
                    }
                    return archiveFilePath;
                }
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