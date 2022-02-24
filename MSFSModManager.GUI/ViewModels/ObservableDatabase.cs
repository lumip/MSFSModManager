// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

using System;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using DynamicData;
using System.Reactive;

namespace MSFSModManager.GUI.ViewModels
{

    class PackageCommandFactory
    {
        private ReactiveCommand<(string, string), Unit> _openAddPackageSourceDialogCommand;
        private ReactiveCommand<InstalledPackage, Unit> _removePackageSourceCommand;
        private ReactiveCommand<InstalledPackage, Unit> _uninstallPackageSourceCommand;

        public PackageCommandFactory(
            ReactiveCommand<(string, string), Unit> openAddPackageSourceDialogCommand, 
            ReactiveCommand<InstalledPackage, Unit> removePackageSourceCommand,
            ReactiveCommand<InstalledPackage, Unit> uninstallPackageSourceCommand)
        {
            _openAddPackageSourceDialogCommand = openAddPackageSourceDialogCommand;
            _removePackageSourceCommand = removePackageSourceCommand;
            _uninstallPackageSourceCommand = uninstallPackageSourceCommand;
        }

        public IReactiveCommand GetOpenAddPackageSourceDialogCommand(InstalledPackage package)
        {
            return ReactiveCommand.CreateFromTask(
                async () => await _openAddPackageSourceDialogCommand.Execute(
                    (package.Id, package.PackageSource?.AsSourceString() ?? "")
                )
            );
        }

        public IReactiveCommand GetRemovePackageSourceCommand(InstalledPackage package)
        {
            return ReactiveCommand.CreateFromTask(async () => await _removePackageSourceCommand.Execute(package));
        }

        public IReactiveCommand GetUninstallPackageCommand(InstalledPackage package)
        {
            return ReactiveCommand.CreateFromTask(async () => await _uninstallPackageSourceCommand.Execute(package));
        } 
    }

    class ObservableDatabase : ReactiveObject
    {

        private IPackageDatabase _database;
        public IPackageDatabase Database
        {
            get => _database;
            set
            {
                this.RaisePropertyChanging(nameof(Database));
                _database = value;
                Packages.Clear();
                Packages.AddOrUpdate(_database.Packages.Select(CreateViewModel));
                
                this.RaisePropertyChanged(nameof(Database));
            }
        }

        public SourceCache<PackageViewModel, string> Packages { get; }
        public IObservable<IChangeSet<PackageViewModel, string>> Connect() => Packages.Connect();


        private PackageCommandFactory _packageCommandFactory;
        private PackageVersionCache _versionCache;
        private AvailableVersionFetchingProgressViewModel _versionFetchingProgressViewModel;

        public ObservableDatabase(
            IPackageDatabase database,
            PackageCommandFactory packageCommandFactory,
            PackageVersionCache versionCache,
            AvailableVersionFetchingProgressViewModel versionFetchingProgressViewModel)
        {
            _packageCommandFactory = packageCommandFactory;
            _versionCache = versionCache;
            _versionFetchingProgressViewModel = versionFetchingProgressViewModel;

            Packages = new SourceCache<PackageViewModel, string>(p => p.Id);
            _database = database;
        }

        private PackageViewModel CreateViewModel(InstalledPackage package)
        {
            return new PackageViewModel(
                    package,
                    _packageCommandFactory.GetOpenAddPackageSourceDialogCommand(package),
                    _packageCommandFactory.GetRemovePackageSourceCommand(package),
                    _packageCommandFactory.GetUninstallPackageCommand(package),
                    _versionCache,
                    _versionFetchingProgressViewModel
            );
        }

        private void AddOrUpdateViewModel(InstalledPackage package)
        {
            PackageViewModel pvm = CreateViewModel(package);

            var optionalPvm = Packages.Lookup(package.Id);
            if (optionalPvm.HasValue)
                pvm.MarkedForInstall = optionalPvm.Value.MarkedForInstall;

            Packages.AddOrUpdate(pvm);
        }

        public void AddPackageSource(IPackageSource source)
        {
            _database.AddPackageSource(source.PackageId, source);

            InstalledPackage p = _database.GetInstalledPackage(source.PackageId);
            AddOrUpdateViewModel(p);
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
                AddOrUpdateViewModel(p);
            }
        }

        public async Task InstallPackage(
            IPackageInstaller installer, IProgressMonitor monitor, CancellationToken cancellationToken
        )
        {
            await _database.InstallPackage(installer, monitor, cancellationToken);
            InstalledPackage p = _database.GetInstalledPackage(installer.PackageId);
            AddOrUpdateViewModel(p);
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
                AddOrUpdateViewModel(p);
            }
        }

        public PackageViewModel GetInstalledPackage(string packageId)
        {
            var optionalPvm = Packages.Lookup(packageId);
            if (optionalPvm.HasValue)
            {
                return optionalPvm.Value;
            }
            throw new PackageNotInstalledException(packageId);
        }

        public bool Contains(string packageId)
        {
            return _database.Contains(packageId);
        }
    }
}
