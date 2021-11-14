using System; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{
        
    public interface IPackageSource : IJsonSerializable
    {
        Task<PackageManifest> GetPackageManifest(IProgressMonitor? monitor = null);
        Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IProgressMonitor? monitor = null);

        IPackageInstaller GetInstaller(VersionNumber versionNumber);

        Task<IEnumerable<VersionNumber>> ListAvailableVersions();

    }

}