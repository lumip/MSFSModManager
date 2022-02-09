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
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using System.Diagnostics.CodeAnalysis;

namespace MSFSModManager.GUI.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {

        private PackageDatabase _database;
        private ObservableDatabase _observableDatabase;
        
        private IPackageSourceRegistry _packageSourceRegistry;

        private PackageVersionCache _latestVersionCache;


#region Direct UI properties
        public LogViewModel Log { get; }

        public AvailableVersionFetchingProgressViewModel VersionFetchingProgress { get; }

        public IVersionNumber GameVersion { get; }

        private bool _includeSystemPackages;
        public bool IncludeSystemPackages
        {
            get => _includeSystemPackages;
            set => this.RaiseAndSetIfChanged(ref _includeSystemPackages, value);
        }

        private bool _onlyWithSource;
        public bool OnlyWithSource
        {
            get => _onlyWithSource;
            set => this.RaiseAndSetIfChanged(ref _onlyWithSource, value);
        }

        private string _filterString;
        public string FilterString
        {
            get => _filterString;
            set => this.RaiseAndSetIfChanged(ref _filterString, value);
        }

        public List<string> PackageTypes { get; }
        private int _typeFilterIndex;
        public int TypeFilterIndex
        {
            get => _typeFilterIndex;
            set => this.RaiseAndSetIfChanged(ref _typeFilterIndex, value);
        }
        
        public const string TYPE_FILTER_ALL_STRING = "ALL";
        public const int TYPE_FILTER_ALL_INDEX = 0;
        
        private PackageViewModel? _selectedPackage;
        public PackageViewModel? SelectedPackage
        {
            get => _selectedPackage;
            set => this.RaiseAndSetIfChanged(ref _selectedPackage, value);
        }

        private string ContentPath { get; }
        
#endregion

#region Derived UI Properties
        private IDisposable _filteredPackagesPipeline;
        private ReadOnlyObservableCollection<PackageViewModel> _filteredPackages;
        public ReadOnlyObservableCollection<PackageViewModel> FilteredPackages => _filteredPackages;

        private IDisposable _installationCandidatesPipeline;
        private readonly ReadOnlyObservableCollection<PackageViewModel> _installationCandidates;

#endregion
        
#region UI Commands
        public Interaction<AddPackageViewModel, AddPackageDialogReturnValues> AddPackageDialogInteraction { get; }
        public ReactiveCommand<Unit, Unit> OpenAddPackageDialogCommand { get; }
        public ReactiveCommand<InstalledPackage, Unit> RemovePackageSourceCommand { get; }

        public ReactiveCommand<InstalledPackage, Unit> UninstallPackageCommand { get; }

        public ICommand InstallSelectedPackagesCommand { get; }

        public Interaction<InstallDialogViewModel, IEnumerable<string>> InstallPackagesDialogInteraction { get; }
#endregion


        public MainWindowViewModel(
            PackageDatabase database, PackageSourceRegistry packageSourceRegistry, IVersionNumber gameVersion, LogViewModel log, string contentPath)
        {
            _latestVersionCache = new PackageVersionCache();
            VersionFetchingProgress = new AvailableVersionFetchingProgressViewModel();


            _observableDatabase = new ObservableDatabase(
                database,
                new PackageCommandFactory(this),
                _latestVersionCache,
                VersionFetchingProgress
            );
            
            _packageSourceRegistry = packageSourceRegistry;
            

            GameVersion = gameVersion;
            ContentPath = contentPath;

            _database = database;
            Log = log;

            _filterString = string.Empty;

            var typesInDatabase = _database.Packages.Select(p => p.Type).ToHashSet();
            var types = new List<string>();
            types.Add(TYPE_FILTER_ALL_STRING);
            types.AddRange(typesInDatabase);
            PackageTypes = types;


            AddPackageDialogInteraction = new Interaction<AddPackageViewModel, AddPackageDialogReturnValues>();

            OpenAddPackageDialogCommand = ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog());
            RemovePackageSourceCommand = ReactiveCommand.Create<InstalledPackage, Unit>(p => {
                _observableDatabase.RemoveSource(p);
                return Unit.Default;
            });
            UninstallPackageCommand = ReactiveCommand.CreateFromTask<InstalledPackage, Unit>(async p => {
                await DoUninstallPackage(p);
                return Unit.Default;
            });

            IncludeSystemPackages = false;
            OnlyWithSource = false;

            var packageFilterFunction = this
                .WhenAnyValue(x => x.FilterString, x => x.TypeFilterIndex, x => x.OnlyWithSource, x => x.IncludeSystemPackages, MakeFilter);

            _filteredPackagesPipeline = _observableDatabase.Connect()
                .Filter(packageFilterFunction)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filteredPackages)
                .Subscribe();

            _installationCandidatesPipeline = _observableDatabase.Connect()
                .AutoRefresh(pvm => pvm.MarkedForInstall)
                .Filter(pvm => pvm.MarkedForInstall)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _installationCandidates)
                .Subscribe();

            InstallPackagesDialogInteraction = new Interaction<InstallDialogViewModel, IEnumerable<string>>();
            InstallSelectedPackagesCommand = ReactiveCommand.CreateFromTask(
                DoOpenInstallDialog,
                _installationCandidates.WhenAnyValue(x => x.Count).Select(c => c > 0)
            );
        }

        public async Task DoOpenAddPackageDialog(string packageId = "", string packageSourceString = "")
        {
            var dialog = new AddPackageViewModel(_database, _packageSourceRegistry, packageId, packageSourceString);
            var addPackageDialogReturn = await AddPackageDialogInteraction.Handle(dialog);

            if (addPackageDialogReturn?.PackageSource != null)
            {
                _observableDatabase.AddPackageSource(addPackageDialogReturn.PackageSource);
                
                if (addPackageDialogReturn.MarkForInstallation)
                {
                    PackageViewModel package = _observableDatabase.GetInstalledPackage(addPackageDialogReturn.PackageSource.PackageId);
                    package.MarkedForInstall = true;

                    if (addPackageDialogReturn.InstallAfterAdding)
                        InstallSelectedPackagesCommand.Execute(null);
                }
            }
        }

        private async Task DoOpenInstallDialog()
        {
            var dialog = new InstallDialogViewModel(_observableDatabase, _installationCandidates.Select(pvm => pvm.Package), GameVersion);
            var installedPackages = await InstallPackagesDialogInteraction.Handle(dialog);

            // clear all successfully installed packages from the installation candidates
            foreach (var pvm in _installationCandidates.ToArray().Join(installedPackages, pvm => pvm.Id, id => id, (pvm, _) => pvm))
            {
                pvm.MarkedForInstall = false;
            }
        }

        private async Task DoUninstallPackage(InstalledPackage package)
        {
            await Task.Run(() => _observableDatabase.Uninstall(package));
        }

        private Func<PackageViewModel, bool> MakeFilter(string filterString, int filterTypeIndex, bool onlyWithSource, bool includeSystemPackages)
        {
            return pvm => 
                    (includeSystemPackages || pvm.Package.IsCommunityPackage) &&
                    (string.IsNullOrWhiteSpace(filterString) || pvm.Package.Id.Contains(filterString)) &&
                    (filterTypeIndex == TYPE_FILTER_ALL_INDEX ||
                     pvm.Package.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
                    (!onlyWithSource || pvm.Package.PackageSource != null);
        }

        public void ClearFilters()
        {
            FilterString = string.Empty;
            TypeFilterIndex = 0;
        }

    }
}
