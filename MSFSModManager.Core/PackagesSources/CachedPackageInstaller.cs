// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MSFSModManager.Core.PackageSources
{

    public class CachedPackageInstaller : IPackageInstaller
    {
        private string _sourcePath;
        public string PackageId { get; }

        public CachedPackageInstaller(string packageId, string sourcePath)
        {
            _sourcePath = sourcePath;
            PackageId = packageId;
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

        public Task Install(string destination, IProgressMonitor? monitor)
        {
            CopyDirectory(_sourcePath, destination);
            return Task.CompletedTask;
        }
    }

}