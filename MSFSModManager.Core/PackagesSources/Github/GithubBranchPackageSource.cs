using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.Parsing;

namespace MSFSModManager.Core.PackageSources.Github
{
    public class GithubBranchPackageSource : AbstractPackageSource
    {
        private string _packageId;

        private GithubRepository _repository;
        private string _branch;

        private PackageCache _cache;

        private HttpClient _client;

        public GithubBranchPackageSource(string packageId, GithubRepository repository, string branchName, PackageCache cache, HttpClient client)
        {
            _packageId = packageId;
            _repository = repository;
            _branch = branchName;
            _cache = cache;
            _client = client;
            _lastReturned = null;
        }

        private List<GithubAPI.Commit>? _branchCommits;

        private async Task<IEnumerable<GithubAPI.Commit>> FetchCommits()
        {
            if (_branchCommits == null)
            {
                _branchCommits = (await GithubAPI.GetBranchCommits(_repository, _branch)).ToList();
            }
            return _branchCommits;
        }

        private GithubAPI.Commit? _lastReturned; // todo: this is to remember which actual commit was (last) checked in dependency resolution (and is therefore the correct one).
        // GetInstaller will get the version number from the manifest, which will not be indicative of the commit!
        // HACKY WORKAROUND THAT MAY BREAK

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Info, versionNumber.ToString());
            GlobalLogger.Log(LogLevel.Warning, "Installing from github branch does not check version bounds, will install latest commit!");
            
            if (!_lastReturned.HasValue) throw new VersionNotAvailableException(_packageId, versionNumber);

            GithubAPI.Commit commit = _lastReturned.Value;

            string downloadUrl = $"https://api.github.com/repos/{_repository.Organisation}/{_repository.Name}/zipball/{commit.Sha}";
            GithubReleaseDownloader downloader = new GithubReleaseDownloader(
                _packageId, versionNumber, _repository, downloadUrl, _client, _cache
            );
            return new GithubReleasePackageInstaller(downloader);

            // if (_releaseCache.ContainsKey(versionNumber))
            // {
            //     CachedRelease cachedRelease = _releaseCache[versionNumber];
            //     GithubReleaseDownloader downloader = new GithubReleaseDownloader(
            //         _packageId, versionNumber, _repository, cachedRelease.DownloadUrl, _client, _cache
            //     );
            //     return new GithubReleasePackageInstaller(downloader);
            // }
            // else
            // {
            //     IEnumerable<GithubAPI.Release> releases = FetchGithubReleases().Result;
            //     foreach (GithubAPI.Release release in releases)
            //     {
            //         VersionNumber releaseVersion;
            //         try
            //         {
            //             releaseVersion = VersionNumber.FromString(release.Name);
            //         }
            //         catch (FormatException)
            //         {
            //             GlobalLogger.Log(LogLevel.Info, $"Cannot parse version number of github release {release.Name} ({_packageId})");
            //             continue;
            //         }
            //         if (versionNumber.Equals(releaseVersion))
            //         {
            //             GithubReleaseDownloader downloader = new GithubReleaseDownloader(
            //                 _packageId, versionNumber, _repository, release.DownloadUrl, _client, _cache
            //             );
            //             return new GithubReleasePackageInstaller(downloader);
            //         }
            //     }
            //     throw new VersionNotAvailableException(_packageId, new VersionBounds(versionNumber));
            // }
        }

        public override async Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor = null)
        {
            foreach (var commit in await FetchCommits())
            {
                if (!versionBounds.CheckVersion(new GitCommitVersionNumber(commit.Sha, commit.Date)))
                    continue;

                string rawManifest = await GithubAPI.GetManifestString(_repository, commit.Sha);
                PackageManifest manifest;
                try
                {
                    manifest = PackageManifest.Parse(_packageId, rawManifest);
                }
                catch (ArgumentException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Manifest for {_packageId} github commit {commit.Sha} does not provide a version number.");
                    throw new PackageNotAvailableException(_packageId);
                }

                if (gameVersion.CompareTo(manifest.MinimumGameVersion) < 0)
                {
                    GlobalLogger.Log(LogLevel.Info, $"{_packageId} branch {_branch} (latest commit: {commit.Sha} requires game version {manifest.MinimumGameVersion}, installed is {gameVersion}; skipping.");
                    throw new PackageNotAvailableException(_packageId);
                }
                _lastReturned = commit;
                return manifest;
            }
            throw new VersionNotAvailableException(_packageId, versionBounds);
        }

        public override string ToString()
        {
            return $"https://github.com/{_repository.Organisation}/{_repository.Name} (Branch {_branch})";
        }

        public override async Task<IEnumerable<IVersionNumber>> ListAvailableVersions()
        {
            return (await FetchCommits()).Select(c => new GitCommitVersionNumber(c.Sha, c.Date));
        }

        public override JToken Serialize()
        {
            JObject serialized = new JObject();
            serialized.Add("repository", _repository.Serialize());
            serialized.Add("branch", _branch);
            return serialized;
        }

        public static GithubBranchPackageSource Deserialize(string packageId, JToken serialized, PackageCache cache, HttpClient client)
        {
            JObject? repositoryObj = serialized["repository"] as JObject;
            if (repositoryObj == null) throw new JsonParsingException("no repository present");

            GithubRepository repository = GithubRepository.Deserialize(repositoryObj);

            string branch = JsonUtils.Cast<string>(serialized["branch"]);

            return new GithubBranchPackageSource(
                packageId,
                repository,
                branch,
                cache,
                client
            );
        }
    }
}