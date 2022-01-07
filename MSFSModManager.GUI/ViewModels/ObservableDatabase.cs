// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using DynamicData;

namespace MSFSModManager.GUI.ViewModels
{

    public class ObservableDatabase : ReactiveObject
    {

        private IPackageDatabase _database;
        public IPackageDatabase Database => _database;

        public SourceCache<InstalledPackage, string> Packages { get; }
        public IObservable<IChangeSet<InstalledPackage, string>> Connect() => Packages.Connect();

        public ObservableDatabase(IPackageDatabase database)
        {
            _database = database;
            Packages = new SourceCache<InstalledPackage, string>(p => p.Id);
            Packages.AddOrUpdate(_database.Packages);
        }

        public void AddPackageSource(IPackageSource source)
        {
            _database.AddPackageSource(source.PackageId, source);

            InstalledPackage p = _database.GetInstalledPackage(source.PackageId);
            Packages.AddOrUpdate(p);
        }

        public void RemoveSource(InstalledPackage p)
        {
            _database.RemovePackageSource(p.Id);

            if (!_database.Contains(p.Id))
            {
                Packages.RemoveKey(p.Id);
            }
            else
            {
                Packages.AddOrUpdate(p);
            }
        }

        public async Task InstallPackage(IPackageInstaller installer, IProgressMonitor? monitor = null)
        {
            await _database.InstallPackage(installer, monitor);
            if (!_database.Contains(installer.PackageId))
            {
                Packages.AddOrUpdate(_database.GetInstalledPackage(installer.PackageId));
            }
        }

        public void Uninstall(InstalledPackage p)
        {
            _database.Uninstall(p.Id);

            if (!_database.Contains(p.Id))
            {
                Packages.RemoveKey(p.Id);
            }
            else
            {
                Packages.AddOrUpdate(p);
            }

        }
    }
}
