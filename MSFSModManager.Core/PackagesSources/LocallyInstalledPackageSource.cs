using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace MSFSModManager.Core.PackageSources
{

    class LocallyInstalledPackageSource : AbstractPackageSource
    {

        private InstalledPackage _installedPackage;

        public LocallyInstalledPackageSource(InstalledPackage installedPackage)
        {
            _installedPackage = installedPackage;
            if (_installedPackage.Manifest == null) throw new ArgumentException("Installed package must have a manifest.", nameof(installedPackage));
        }

        public override IPackageInstaller GetInstaller(IVersionNumber versionNumber)
        {
            return new NoOpPackageInstaller(_installedPackage.Id);
        }

        public override Task<PackageManifest> GetPackageManifest(VersionBounds versionBounds, IVersionNumber gameVersion, IProgressMonitor? monitor)
        {
            PackageManifest manifest = _installedPackage.Manifest!;
            if (versionBounds.CheckVersion(manifest.Version))
            {
                return Task.FromResult(manifest);
            }
            throw new VersionNotAvailableException(_installedPackage.Id, versionBounds);
        }

        public override Task<IEnumerable<IVersionNumber>> ListAvailableVersions()
        {
            IVersionNumber[] versions = new VersionNumber[] { _installedPackage.Manifest!.Version };
            return Task.FromResult(versions.AsEnumerable());
        }

        public override JToken Serialize()
        {
            throw new System.NotImplementedException();
        }
    }

}