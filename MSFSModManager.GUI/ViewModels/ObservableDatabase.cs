// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Collections.Specialized;
using System.Collections;
using DynamicData;

namespace MSFSModManager.GUI.ViewModels
{

    public class ObservableDatabase : ReactiveObject/*, INotifyCollectionChanged*/
    {

        public class PackageEnumerable : IEnumerable<InstalledPackage>, INotifyCollectionChanged
        {

            private IEnumerable<InstalledPackage> _packages;

            public PackageEnumerable(IEnumerable<InstalledPackage> packages)
            {
                _packages = packages;
            }

            public event NotifyCollectionChangedEventHandler? CollectionChanged;

            public IEnumerator<InstalledPackage> GetEnumerator()
            {
                return _packages.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_packages).GetEnumerator();
            }

            internal void NotifyPackageAdded(InstalledPackage package)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Add,
                    new List<InstalledPackage>(new InstalledPackage[] { package })
                );
                CollectionChanged?.Invoke(this, args);
            }

            internal void NotifyPackageRemoved(InstalledPackage package)
            {
                var args = new NotifyCollectionChangedEventArgs(
                    NotifyCollectionChangedAction.Remove,
                    new List<InstalledPackage>(new InstalledPackage[] { package })
                );
                CollectionChanged?.Invoke(this, args);
            }

            internal void NotifyPackageUpdated(InstalledPackage package)
            {
                throw new NotImplementedException();
            }
        }

        private IPackageDatabase _database;

        // public event NotifyCollectionChangedEventHandler? CollectionChanged;

        // private ObservableCollection<InstalledPackage> _packages;
        // public ObservableCollection<InstalledPackage> Packages => _packages;
        // public PackageEnumerable Packages { get; }
        // public IEnumerable<InstalledPackage> Packages => _database.Packages;

        public SourceCache<InstalledPackage, string> Packages { get; }
        public IObservable<IChangeSet<InstalledPackage, string>> Connect() => Packages.Connect();

        public ObservableDatabase(IPackageDatabase database)
        {
            _database = database;
            Packages = new SourceCache<InstalledPackage, string>(p => p.Id);
            Packages.AddOrUpdate(_database.Packages);


            // Packages = new PackageEnumerable(_database.Packages);
            // Packages.CollectionChanged += OnPackagesChanged;
            // _packages = new ObservableCollection<InstalledPackage>(_database.Packages);
        }

        // private void OnPackagesChanged(object? sender, NotifyCollectionChangedEventArgs args)
        // {
        //     if (sender == Packages)
        //     {
        //         this.RaisePropertyChanged(nameof(Packages));
        //     }
        // }

        public void AddPackageSource(IPackageSource source)
        {
            // this.RaisePropertyChanging(nameof(Packages));
            bool updated = _database.Contains(source.PackageId);
            _database.AddPackageSource(source.PackageId, source);
            // this.RaisePropertyChanged(nameof(Packages));


            InstalledPackage p = _database.GetInstalledPackage(source.PackageId);
            // if (updated)
            // {
            //     GlobalLogger.Log(LogLevel.Debug, $"Source for existing package was updated: {p.Id} @ {p.PackageSource}");
            //     Packages.NotifyPackageUpdated(p);
            // }
            // else
            // {
            //     GlobalLogger.Log(LogLevel.Debug, $"New package source added: {p.Id} @ {p.PackageSource}");
            //     Packages.NotifyPackageAdded(p);
            // }
            Packages.AddOrUpdate(p);

            // var args = new NotifyCollectionChangedEventArgs(replaced ? NotifyCollectionChangedAction.Replace : NotifyCollectionChangedAction.Add,
            //     new List<InstalledPackage>( new InstalledPackage[] { _database.GetInstalledPackage(source.PackageId) })
            // );
            // CollectionChanged?.Invoke(this.Packages, args);
        }

        public void RemoveSource(InstalledPackage p)
        {
            _database.RemovePackageSource(p.Id);
            // if (_database.Contains(p.Id))
            // {
            //     GlobalLogger.Log(LogLevel.Debug, $"Source for installed package was removed: {p.Id}");
            //     Packages.NotifyPackageUpdated(p);
            // }
            // else
            // {
            //     GlobalLogger.Log(LogLevel.Debug, $"Source and package package were removed: {p.Id}");
            //     Packages.NotifyPackageRemoved(p);
            // }
            if (!_database.Contains(p.Id))
                Packages.RemoveKey(p.Id);
        }
    }
}