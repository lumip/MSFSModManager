using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources
{
    public interface IPackageSourceRegistry
    {
        IPackageSource Deserialize(string packageId, JToken serialized);
        IPackageSource GetSourceForURL(string packageId, string url, HttpClient client);

        IPackageSource ParseSourceStrings(string packageId, string[] sourceString);
    }

}