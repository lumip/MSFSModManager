using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

using MSFSModManager.Core.Parsing;

namespace MSFSModManager.Core.PackageSources.Github
{

    public class GithubRepositoryFormatException : Exception
    {
        public GithubRepositoryFormatException(string message)
            : base(message) { }
    }

    public class GithubAPIException : Exception
    {
        public GithubAPIException(string message, Exception baseException)
            : base($"Could not parse github response: {message}", baseException) { }

        public GithubAPIException(string message)
            : base($"Could not parse github response: {message}") { }

        public GithubAPIException(Exception baseException)
            : base("Could not parse github response.", baseException) { }
    }
    
    public class GithubRepository : IJsonSerializable
    {
        public string Organisation { get; }
        public string Name { get; }

        public IEnumerable<string> WatchedBranches => _watchedBranches;

        private List<string> _watchedBranches;

        public GithubRepository(string repositoryOrganisation, string repositoryName)
        {
            Organisation = repositoryOrganisation;
            Name = repositoryName;
            _watchedBranches = new List<string>();
            _watchedBranches.Add("master");
        }

        public static GithubRepository FromUrl(string repositoryUrl)
        {
            Regex re = new Regex("https://github.com/(?<Organisation>.*)/(?<Name>[A-z0-9]*)");
            var match = re.Match(repositoryUrl);
            if (match.Success)
            {
                string organisation = match.Groups["Organisation"].Value;
                string name = match.Groups["Name"].Value;
                return new GithubRepository(organisation, name);
            }
            throw new Exception("could not parse given url");
        }

        public JToken Serialize()
        {
            JObject serialized = new JObject();
            serialized.Add("organisation", Organisation);
            serialized.Add("name", Name);
            return serialized;
        }

        public static GithubRepository Deserialize(JToken serialized)
        {
            if (!(serialized is JObject)) throw new JsonParsingException("");
            JObject obj = (JObject)serialized;

            if (!(obj.ContainsKey("organisation") && obj.ContainsKey("name")))
                throw new Parsing.JsonParsingException("JSON object is missing at least one of 'organisation' and 'name'.");
            string organisation = (string)obj["organisation"]!;
            string repositoryName = (string)obj["name"]!;

            return new GithubRepository(organisation, repositoryName);
        }
    }

    public class GithubAPI
    {

        static async Task<string> MakeRequest(string requestUrl)
        {
            HttpWebRequest request = WebRequest.CreateHttp(requestUrl);
            request.UserAgent = "lumip";
            request.Headers.Add(HttpRequestHeader.Accept, "application/vnd.github.v3+json");
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responseString = streamReader.ReadToEnd();
            return responseString;
        }

        static public async Task<IEnumerable<string>> FetchReleaseNames(GithubRepository repository)
        {
            List<string> tagNames = new List<string>();

            string requestUrl = $"https://api.github.com/repos/{repository.Organisation}/{repository.Name}/releases";
            string responseString = await MakeRequest(requestUrl);

            try
            {
                foreach (var release in JArray.Parse(responseString).Children())
                {
                    bool isPreRelease = JsonUtils.CastMember<bool>(release, "prerelease");
                    if (isPreRelease == false)
                    {
                        string tagName = JsonUtils.CastMember<string>(release, "tag_name");
                        tagNames.Add(tagName);
                    }
                }
                return tagNames;
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException("While fetching release names.", e);
            }
        }

        public struct Release
        {
            public string Name { get; }
            public string DownloadUrl { get; }

            public Release(string name, string downloadUrl)
            {
                Name = name;
                DownloadUrl = downloadUrl;
            }
        }

        static public async Task<IEnumerable<Release>> FetchReleases(GithubRepository repository, IGithubReleaseArtifactSelector artifactSelector)
        {
            List<Release> releases = new List<Release>();

            string requestUrl = $"https://api.github.com/repos/{repository.Organisation}/{repository.Name}/releases";
            string responseString = await MakeRequest(requestUrl);

            try
            {
                foreach (var release in JArray.Parse(responseString).Children())
                {
                    bool isPreRelease = JsonUtils.CastMember<bool>(release, "prerelease");
                    if (isPreRelease == false)
                    {
                        string tagName = JsonUtils.CastMember<string>(release, "tag_name");

                        JArray assetTokens = JsonUtils.CastMember<JArray>(release, "assets");

                        int artifact;
                        try
                        {
                            artifact = artifactSelector.SelectReleaseArtifact(
                                assetTokens.Select(t => JsonUtils.CastMember<string>(t, "name")
                            ).ToArray());
                        }
                        catch (Exception e)
                        {
                            if (e.Message.Contains("no") || e.Message.Contains("more than"))
                            {
                                GlobalLogger.Log(LogLevel.Warning, $"{e.Message} ({repository.Name}, {tagName})");
                                continue;
                            }
                            throw e;
                        }

                        string url = JsonUtils.CastMember<string>(assetTokens[artifact], "browser_download_url");
                        releases.Add(new Release(
                            tagName, url
                        ));
                    }
                }
                return releases;
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException("While fetching information on available releases.", e);
            }
        }

        public struct Commit
        {
            public readonly string Sha;
            public readonly DateTime Date;

            public Commit(string sha, DateTime date)
            {
                Sha = sha;
                Date = date;
            }
        }

        private static Commit ParseCommit(JToken token)
        {
            try
            {
                string sha = JsonUtils.CastMember<string>(token, "sha");
                
                JToken? commitInfoRaw = token["commit"];
                if (commitInfoRaw == null) throw new JsonParsingException("Commit info does not contain 'commit'.");

                JToken? authorRaw = commitInfoRaw["author"];
                if (authorRaw == null) throw new GithubRepositoryFormatException("Commit info does not contain 'author'.");

                DateTime date = JsonUtils.CastMember<DateTime>(authorRaw, "date");
                return new Commit(sha, date);
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException("While parsing commit.", e);
            }
        }

        public static async Task<IEnumerable<Commit>> GetBranchCommits(GithubRepository repository, string branch)
        {
            string requestUrl = $"https://api.github.com/repos/{repository.Organisation}/{repository.Name}/commits?sha={branch}";
            string responseString = await MakeRequest(requestUrl);

            try
            {
                return JArray.Parse(responseString).Children().Select(t => ParseCommit(t));
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException($"While fetching commits from branch {branch}.", e);
            }
        }

        public static async Task<string> GetCommitShaForTag(GithubRepository repository, string tag)
        {
            string requestUrl = $"https://api.github.com/repos/{repository.Organisation}/{repository.Name}/commits/{tag}";
            string responseString = await MakeRequest(requestUrl);

            try
            {
                JObject commit = JObject.Parse(responseString);
                return JsonUtils.CastMember<string>(commit, "sha");
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException($"While fetching commit sha for tag '{tag}'.", e);
            }
        }

        public static async Task<string> GetManifestString(GithubRepository repository, string commitSha)
        {
            string requestUrl = $"https://api.github.com/repos/{repository.Organisation}/{repository.Name}/git/trees/{commitSha}?recursive=1";
            string responseString = await MakeRequest(requestUrl);          

            var manifestRegex = new Regex(@"manifest.*\.json");

            try
            {
                var manifestFiles = JsonUtils.CastMember<JArray>(JObject.Parse(responseString), "tree")
                    .Select(fileToken => (JsonUtils.CastMember<string>(fileToken, "path"), JsonUtils.CastMember<string>(fileToken, "url")))
                    .Where(fileToken => manifestRegex.IsMatch(fileToken.Item1))
                    .OrderBy(fileToken => Path.GetFileName(fileToken.Item1))
                    .Select(fileToken => fileToken.Item2)
                    .ToArray();

                if (manifestFiles.Length == 0) throw new FileNotFoundException("Manifest file not found in release commit.");

                string manifestUrl = manifestFiles.First();
                responseString = await MakeRequest(manifestUrl);

                JObject manifestObject = JObject.Parse(responseString);
                string manifestRaw = Encoding.UTF8.GetString(
                    Convert.FromBase64String(JsonUtils.CastMember<string>(manifestObject, "content"))
                );
                return manifestRaw;
            }
            catch (Exception e) when (e is JsonParsingException || e is JsonReaderException)
            {
                throw new GithubAPIException($"While reading manifest from repository {repository} for commit {commitSha}.", e);
            }
        }

    }

}