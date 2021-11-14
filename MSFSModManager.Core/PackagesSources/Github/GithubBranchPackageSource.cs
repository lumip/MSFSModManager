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

        public GithubBranchPackageSource(string packageId, GithubRepository repository, string branchName, PackageCache cache)
        {
            _packageId = packageId;
            _repository = repository;
            _branch = branchName;
            _cache = cache;
        }

        private List<string>? _branchCommits;

        private async Task<IEnumerable<string>> FetchCommits()
        {
            if (_branchCommits == null)
            {
                _branchCommits = (await GithubAPI.GetBranchCommits(_repository, _branch)).ToList();
            }
            return _branchCommits;
        }

        public override IPackageInstaller GetInstaller(VersionNumber versionNumber)
        {
            GlobalLogger.Log(LogLevel.Warning, "Installing from github branch does not check version bounds, will install latest commit!");
            string commitSha = FetchCommits().Result.First();

            throw new NotImplementedException(); // todo!
        }

        public override async Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor = null)
        {
            GlobalLogger.Log(LogLevel.Warning, "Sourcing from github branch does not check version bounds, will use latest commit!");

            string commitSha = (await FetchCommits()).First();
            string rawManifest = await GithubAPI.GetManifestString(_repository, commitSha);
            PackageManifest manifest;
            try
            {
                manifest = PackageManifest.Parse(_packageId, rawManifest);
            }
            catch (ArgumentException)
            {
                GlobalLogger.Log(LogLevel.CriticalError, $"Manifest for {_packageId} github commit {commitSha} does not provide a version number.");
                throw new PackageNotAvailableException(_packageId);
            }

            if (gameVersion.CompareTo(manifest.MinimumGameVersion) < 0)
            {
                GlobalLogger.Log(LogLevel.Info, $"{_packageId} branch {_branch} (latest commit: {commitSha} requires game version {manifest.MinimumGameVersion}, installed is {gameVersion}; skipping.");
                throw new PackageNotAvailableException(_packageId);
            }
            return manifest;
        }

        public override string ToString()
        {
            return $"https://github.com/{_repository.Organisation}/{_repository.Name} (Branch {_branch})";
        }

        public override Task<IEnumerable<VersionNumber>> ListAvailableVersions()
        {
            throw new NotSupportedException();
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
                cache
            );
        }
    }
}