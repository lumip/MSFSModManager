// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{

    public class CachedPackageInstaller : IPackageInstaller
    {
        private string _sourcePath;
        private PackageManifest _manifest;
        public string PackageId => _manifest.Id;

        public CachedPackageInstaller(PackageManifest manifest, string sourcePath)
        {
            _sourcePath = sourcePath;
            _manifest = manifest;
        }

        private static void CopyDirectory(string sourcePath, string destinationPath)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourcePath);
            
            Directory.CreateDirectory(destinationPath);

            foreach (var file in sourceDir.GetFiles())
            {
                File.Copy(file.FullName, Path.Join(destinationPath, file.Name));
            }

            foreach (var subdir in sourceDir.GetDirectories())
            {
                CopyDirectory(subdir.FullName, Path.Join(destinationPath, subdir.Name));
            }
        }

        public async Task<PackageManifest> Install(
            string destination, IProgressMonitor? monitor, CancellationToken cancellationToken
        )
        {
            await Task.Run(() => CopyDirectory(_sourcePath, destination), cancellationToken);
            return _manifest;
        }
    }

}