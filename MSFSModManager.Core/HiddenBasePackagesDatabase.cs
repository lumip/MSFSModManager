using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MSFSModManager.Core.PackageSources;

namespace MSFSModManager.Core
{
    public class HiddenBasePackagesDatabase : IPackageDatabase
    {

        IPackageDatabase _database;

        public HiddenBasePackagesDatabase(IPackageDatabase database)
        {
            _database = database;
        }

        public IEnumerable<InstalledPackage> Packages => _database.Packages;

        public IEnumerable<InstalledPackage> CommunityPackages => _database.CommunityPackages;

        public IEnumerable<InstalledPackage> OfficialPackages => _database.OfficialPackages;

        public void AddPackageSource(string packageId, IPackageSource packageSource)
        {
            if (packageId.StartsWith("fs-base"))
            {
                throw new NotSupportedException("Cannot manage sources of fs-base packages.");
            }
            _database.AddPackageSource(packageId, packageSource);
        }

        public bool Contains(string packageId)
        {
            return Contains(packageId, VersionBounds.Unbounded);
        }

        public bool Contains(string packageId, VersionBounds versionBounds)
        {
            return (packageId.StartsWith("fs-base")) || _database.Contains(packageId, versionBounds);
        }

        public InstalledPackage GetInstalledPackage(string packageId)
        {
            if (packageId.StartsWith("fs-base"))
            {
                throw new NotSupportedException($"Cannot get installation info for fs-base packages.");
            }
            return _database.GetInstalledPackage(packageId);
        }

        public Task InstallPackage(IPackageInstaller installer, IProgressMonitor? monitor = null)
        {
            if (installer.PackageId.StartsWith("fs-base"))
            {
                throw new NotSupportedException("Cannot install fs-base packages.");
            }
            return _database.InstallPackage(installer, monitor);
        }

        public void RemovePackageSource(string packageId)
        {
            if (packageId.StartsWith("fs-base"))
            {
                throw new NotSupportedException("Cannot manage sources of fs-base packages.");
            }
            _database.RemovePackageSource(packageId);
        }

        public void Uninstall(string packageId)
        {
            if (packageId.StartsWith("fs-base"))
            {
                throw new NotSupportedException("Cannot uninstall fs-base packages.");
            }
            _database.Uninstall(packageId);
        }
    }
}
