// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using MSFSModManager.GUI.Settings;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;

namespace MSFSModManager.GUI.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {
        private ObservableDatabase _observableDatabase;
        
        public IPackageSourceRegistry PackageSourceRegistry { get; private set; }

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

        private bool _filterHasSource;
        public bool FilterHasSource
        {
            get => _filterHasSource;
            set => this.RaiseAndSetIfChanged(ref _filterHasSource, value);
        }

        private bool _filterUpdateAvailable;
        public bool FilterUpdateAvailable
        {
            get => _filterUpdateAvailable;
            set => this.RaiseAndSetIfChanged(ref _filterUpdateAvailable, value);
        }

        private string _filterString;
        public string FilterString
        {
            get => _filterString;
            set => this.RaiseAndSetIfChanged(ref _filterString, value);
        }

        private int _typeFilterIndex;
        public int TypeFilterIndex
        {
            get => _typeFilterIndex;
            set => this.RaiseAndSetIfChanged(ref _typeFilterIndex, value);
        }
        
        public const string TYPE_FILTER_ALL_STRING = "ALL";
        public const int TYPE_FILTER_ALL_INDEX = 0;

        private IDisposable _packageTypesCachePipeline;
        private SourceList<string> _packageTypesCache;
        private IDisposable _packageTypesPipeline;
        private ReadOnlyObservableCollection<string> _packageTypes;
        public ReadOnlyObservableCollection<string> PackageTypes => _packageTypes;

        
        private ViewModelBase? _selectedPackage;
        public ViewModelBase? SelectedPackage
        {
            get => _selectedPackage;
            set => this.RaiseAndSetIfChanged(ref _selectedPackage, value);
        }


        private readonly ObservableAsPropertyHelper<string> _contentPath;
        public string ContentPath => _contentPath.Value;

        private UserSettings _settings;
        private UserSettings Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }
        
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

        public Interaction<SettingsViewModel, UserSettings> SettingsDialogInteraction { get; }
        public ReactiveCommand<Unit, Unit> OpenSettingsDialogCommand { get; }
#endregion
        public MainWindowViewModel(
            UserSettings settings,
            PackageSourceRegistry packageSourceRegistry,
            IVersionNumber gameVersion,
            LogViewModel log,
            PackageVersionCache latestVersionCache)
        {
            _settings = settings;
            _latestVersionCache = latestVersionCache;
            PackageSourceRegistry = packageSourceRegistry;

            GameVersion = gameVersion;
            Log = log;

            VersionFetchingProgress = new AvailableVersionFetchingProgressViewModel();

            // create package specific commands - these must be initialised before loading up the package database
            AddPackageDialogInteraction = new Interaction<AddPackageViewModel, AddPackageDialogReturnValues>();

            OpenAddPackageDialogCommand = ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog());
            RemovePackageSourceCommand = ReactiveCommand.Create<InstalledPackage, Unit>(p => {
                DoRemovePackageSource(p);
                return Unit.Default;
            });
            UninstallPackageCommand = ReactiveCommand.CreateFromTask<InstalledPackage, Unit>(async p => {
                await DoUninstallPackage(p);
                return Unit.Default;
            });

            // set up view model wrapper for PackageDatabase
            _observableDatabase = new ObservableDatabase(
                new PackageDatabase(_settings.ContentPath, packageSourceRegistry),
                new PackageCommandFactory(
                    ReactiveCommand.CreateFromTask(async ((string, string) args) => await DoOpenAddPackageDialog(args.Item1, args.Item2)),
                    RemovePackageSourceCommand,
                    UninstallPackageCommand
                ),
                _latestVersionCache,
                VersionFetchingProgress    
            );

            // mirror Settings.ContentPath in ContentPath property
            _contentPath = this
                .WhenAnyValue(x => x.Settings.ContentPath)
                .ToProperty(this, x => x.ContentPath, out _contentPath);

            // any change to ContentPath requires reloading the PackageDatabase
            this.WhenAnyValue(x => x.ContentPath).Subscribe(
                cp => { _observableDatabase.Database = new PackageDatabase(cp, PackageSourceRegistry); });

            // Setting up package filters below            
            IncludeSystemPackages = false;
            FilterHasSource = false;
            FilterUpdateAvailable = false;

            _filterString = string.Empty;
            _typeFilterIndex = TYPE_FILTER_ALL_INDEX;

            // dynamically extract all package types in database for package type filter into local cache
            _packageTypesCache = new SourceList<string>();
            _packageTypesCache.Insert(TYPE_FILTER_ALL_INDEX, TYPE_FILTER_ALL_STRING); // magic "ALL" type

            _packageTypesCachePipeline = _observableDatabase.Connect()
                .DistinctValues(pvm => pvm.Type)
                .Sort(SortExpressionComparer<string>.Ascending(k => k))
                .Subscribe(types => {
                    _packageTypesCache.Clear();
                    _packageTypesCache.Insert(TYPE_FILTER_ALL_INDEX, TYPE_FILTER_ALL_STRING);

                    _packageTypesCache.AddRange(types.SortedItems.Select(pair => pair.Value));
                });

            // bind package type cache to observable collection property PackageTypes for View
            _packageTypesPipeline = _packageTypesCache.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _packageTypes)
                .ToCollection()
                .Subscribe();

            // update package filter function whenever a filter is changed
            var packageFilterFunction = this
                .WhenAnyValue(
                    x => x.FilterString,
                    x => x.TypeFilterIndex,
                    x => x.FilterHasSource,
                    x => x.IncludeSystemPackages,
                    x => x.FilterUpdateAvailable,
                    MakeFilter
                );

            // filter packages from database to display using filter function
            _filteredPackagesPipeline = _observableDatabase.Connect()
                .Filter(packageFilterFunction)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filteredPackages)
                .Subscribe();

            // dynamically updated collection of all packages marked for installation
            _installationCandidatesPipeline = _observableDatabase.Connect()
                .AutoRefresh(pvm => pvm.MarkedForInstall)
                .Filter(pvm => pvm.MarkedForInstall)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _installationCandidates)
                .Subscribe();

            // set up remaining UI commands
            InstallPackagesDialogInteraction = new Interaction<InstallDialogViewModel, IEnumerable<string>>();
            InstallSelectedPackagesCommand = ReactiveCommand.CreateFromTask(
                DoOpenInstallDialog,
                _installationCandidates.WhenAnyValue(x => x.Count).Select(c => c > 0)
            );
            
            SettingsDialogInteraction = new Interaction<SettingsViewModel, UserSettings>();
            OpenSettingsDialogCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(
                async settingsBuilder => { await DoOpenSettingsDialog(); return Unit.Default; }
            );

            _selectedPackage = new NullPackageViewModel();
        }

        public async Task DoOpenAddPackageDialog(string packageId = "", string packageSourceString = "")
        {
            var dialog = new AddPackageViewModel(_observableDatabase, PackageSourceRegistry, packageId, packageSourceString);
            var addPackageDialogReturn = await AddPackageDialogInteraction.Handle(dialog);

            if (addPackageDialogReturn?.PackageSource != null)
            {
                _latestVersionCache.RemoveCachedVersion(addPackageDialogReturn.PackageSource.PackageId);
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

        public void DoRemovePackageSource(InstalledPackage package)
        {
            _latestVersionCache.RemoveCachedVersion(package);
            _observableDatabase.RemoveSource(package);
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

        private async Task DoOpenSettingsDialog()
        {
            UserSettingsBuilder settingsBuilder = UserSettingsBuilder.LoadFromSettings(Settings);
            var dialog = new SettingsViewModel(settingsBuilder);
            var newSettings = await SettingsDialogInteraction.Handle(dialog);

            if (newSettings != null)
            {
                Settings = newSettings;
                Settings.Save();
            }
        }

        private Func<PackageViewModel, bool> MakeFilter(string filterString, int filterTypeIndex, bool filterHasSource, bool includeSystemPackages, bool filterUpdateAvailable)
        {
            return pvm => 
                    (includeSystemPackages || pvm.Package.IsCommunityPackage) &&
                    (string.IsNullOrWhiteSpace(filterString) || pvm.Package.Id.Contains(filterString)) &&
                    (filterTypeIndex == TYPE_FILTER_ALL_INDEX || filterTypeIndex == -1 ||
                     pvm.Package.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
                    (!filterHasSource || pvm.Package.PackageSource != null) &&
                    (!filterUpdateAvailable || pvm.IsLatestVersionNewer);
        }

        public void ClearFilters()
        {
            FilterString = string.Empty;
            TypeFilterIndex = 0;
            FilterHasSource = false;
            FilterUpdateAvailable = false;
        }

    }
}
