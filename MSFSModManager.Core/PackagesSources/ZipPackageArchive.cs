// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.IO.Compression;

namespace MSFSModManager.Core.PackageSources
{
    public class ZipPackageArchive : IPackageArchive
    {
        private string PathToArchive { get; }

        private ZipArchive _archive;

        private ZipArchiveEntry _manifestEntry;
        private string _packagePathInArchive;

        private static ZipArchiveEntry GetManifestEntry(ZipArchive archive)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.Name == PackageDirectoryLayout.ManifestFile)
                {
                    return entry;
                }
            }
            throw new MalformedArchiveException();
        }

        public ZipPackageArchive(string pathToArchive)
        {
            PathToArchive = pathToArchive;
            try
            {
                _archive = ZipFile.OpenRead(PathToArchive);

                _manifestEntry = GetManifestEntry(_archive);
                _packagePathInArchive = Path.GetDirectoryName(_manifestEntry.FullName);
            }
            catch (InvalidDataException)
            {
                throw new MalformedArchiveException();
            }
            catch (NotSupportedException)
            {
                throw new MalformedArchiveException();
            }
        }

        public IEnumerable<string> Entries
        {
            get
            {
                return _archive.Entries
                        .Where(entry => entry.FullName.StartsWith(_packagePathInArchive))
                        .Select(entry => Path.GetRelativePath(_packagePathInArchive, entry.FullName));
            }
        }

        public Stream OpenManifest()
        {
            return _manifestEntry.Open();
        }

        public void Dispose()
        {
            _archive.Dispose();
        }

        public void Extract(string destination)
        {
            foreach (ZipArchiveEntry entry in _archive.Entries)
            {
                if (entry.FullName.StartsWith(_packagePathInArchive))
                {
                    string relativePath = Path.GetRelativePath(_packagePathInArchive, entry.FullName);

                    string outputPath = Path.Combine(destination, relativePath);
                    if (entry.FullName.EndsWith(Path.DirectorySeparatorChar) ||
                        entry.FullName.EndsWith(Path.AltDirectorySeparatorChar))
                    {
                        Directory.CreateDirectory(outputPath);
                    }
                    else
                    {
                        entry.ExtractToFile(outputPath);
                    }                    
                }
            }
        }

    }
}
