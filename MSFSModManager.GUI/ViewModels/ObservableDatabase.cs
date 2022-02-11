// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using DynamicData;

namespace MSFSModManager.GUI.ViewModels
{

    class PackageCommandFactory
    {
        private MainWindowViewModel _mainViewModel;

        public PackageCommandFactory(MainWindowViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        public IReactiveCommand GetOpenAddPackageSourceDialogCommand(InstalledPackage package)
        {
            return ReactiveCommand.CreateFromTask(
                async () => await _mainViewModel.DoOpenAddPackageDialog(
                    package.Id, package.PackageSource?.AsSourceString() ?? ""
                )
            );
        }

        public IReactiveCommand GetRemovePackageSourceCommand(InstalledPackage package)
        {
            // note(lumip): RemovePackageSourceCommand was created with ReactiveCommand.Create, therefore needs to be subscribed
            //      (which we do by awaiting it), as opposed to the other commands, which were created using
            //      ReactiveCommand.CreateFromTask (why?? how is this allowed to make a difference here?)
            //      This is tricky because the failure mode is that the command simply does not execute (no error/exception happens)
            return ReactiveCommand.Create(async () => await _mainViewModel.RemovePackageSourceCommand.Execute(package));
        }

        public IReactiveCommand GetUninstallPackageCommand(InstalledPackage package)
        {
            return ReactiveCommand.Create(() => _mainViewModel.UninstallPackageCommand.Execute(package));
        } 
    }

    class ObservableDatabase : ReactiveObject
    {

        private IPackageDatabase _database;
        public IPackageDatabase Database => _database;

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
            _database = database;

            _packageCommandFactory = packageCommandFactory;
            _versionCache = versionCache;
            _versionFetchingProgressViewModel = versionFetchingProgressViewModel;

            Packages = new SourceCache<PackageViewModel, string>(p => p.Id);
            Packages.AddOrUpdate(
                _database.Packages.Select(CreateViewModel)
            );
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
    }
}
