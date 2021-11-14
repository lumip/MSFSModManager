using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MSFSModManager.Core.PackageSources
{
    public abstract class AbstractPackageSource : IPackageSource
    {
        public virtual Task<PackageManifest> GetPackageManifest(IVersionNumber gameVersion, IProgressMonitor? monitor = null) => GetPackageManifest(VersionBounds.Unbounded, gameVersion, monitor);
        public abstract Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor = null);

        public abstract IPackageInstaller GetInstaller(VersionNumber versionNumber);

        public abstract JToken Serialize();

        public abstract Task<IEnumerable<VersionNumber>> ListAvailableVersions();
    }
}