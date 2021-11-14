using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    public static class PackageDatabaseExporter
    {
        public static string SerializeDatabaseExport(IPackageDatabase database, bool onlyWithSources, bool ignoreVersion)
        {
            JObject jsonRoot = new JObject();
            foreach (var p in database.CommunityPackages)
            {
                if (p.Manifest == null) continue;

                JObject packageInfoJson = new JObject();
                if (!ignoreVersion)
                {
                    packageInfoJson.Add("version", p.Version!.ToString());
                }

                if (p.PackageSource != null)
                {
                    packageInfoJson.Add("source", PackageSourceRegistry.Serialize(p.PackageSource));
                }
                else if (onlyWithSources) continue;

                jsonRoot.Add(p.Id, packageInfoJson);
            }
            return jsonRoot.ToString(Formatting.Indented);
        }
    }
}