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

namespace MSFSModManager.GUI.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {

        private PackageDatabase _database;
        private ObservableDatabase _observableDatabase;
        private ObservableDatabase ObservableDatabase => _observableDatabase;

        // private ObservableCollection<InstalledPackage> _packages;
        // private ObservableCollection<InstalledPackage> Packages
        // {
        //     get => _packages;
        //     set => this.RaiseAndSetIfChanged(ref _packages, value);
        // }

        // private readonly ObservableAsPropertyHelper<IEnumerable<PackageViewModel>> _filteredPackages;
        // public IEnumerable<PackageViewModel> FilteredPackages => _filteredPackages.Value;

        private PackageViewModel? _selectedPackage;
        public PackageViewModel? SelectedPackage
        {
            get => _selectedPackage;
            set => this.RaiseAndSetIfChanged(ref _selectedPackage, value);
        }

        public LogViewModel Log { get; }

        public Interaction<AddPackageViewModel, IPackageSource?> ShowAddPackageDialog { get; }
        
        public ReactiveCommand<Unit, Unit> OpenAddPackageDialogCommand { get; }
        public ReactiveCommand<InstalledPackage, Unit> RemovePackageSourceCommand { get; }

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

        private IPackageSourceRegistry _packageSourceRegistry;

        private PackageVersionCache _latestVersionCache;

        private SourceCache<InstalledPackage, string> _packagesCache;
        private ReadOnlyObservableCollection<PackageViewModel> _filteredPackages;
        public ReadOnlyObservableCollection<PackageViewModel> FilteredPackages => _filteredPackages;
        private IDisposable _dynamicDataPackages;

        public MainWindowViewModel(PackageDatabase database, PackageSourceRegistry packageSourceRegistry, LogViewModel log)
        {

            // _packages = new ObservableCollection<InstalledPackage>(database.Packages);
            _observableDatabase = new ObservableDatabase(database);
            _packageSourceRegistry = packageSourceRegistry;
            _latestVersionCache = new PackageVersionCache();

            _database = database;
            Log = log;

            _filterString = string.Empty;

            var typesInDatabase = _database.Packages.Select(p => p.Type).ToHashSet();
            var types = new List<string>();
            types.Add(TYPE_FILTER_ALL_STRING);
            types.AddRange(typesInDatabase);
            PackageTypes = types;


            ShowAddPackageDialog = new Interaction<AddPackageViewModel, IPackageSource?>();
            // OpenAddPackageDialogCommand = ReactiveCommand.CreateFromTask(async (args) => {
            //     var dialog = new AddPackageViewModel(_database, packageSourceRegistry, "");
            //     var packageSource = await ShowAddPackageDialog.Handle(dialog);
            //     if (packageSource != null)
            //     {
            //         this.RaisePropertyChanging(nameof(ObservableDatabase));
            //         _observableDatabase.AddPackageSource(packageSource);
            //         // _database.AddPackageSource(packageSource.PackageId, packageSource);
            //         // Packages = new ObservableCollection<InstalledPackage>(database.Packages);
            //         this.RaisePropertyChanged(nameof(ObservableDatabase));
            //     }
            // });

            OpenAddPackageDialogCommand = ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog());
            RemovePackageSourceCommand = ReactiveCommand.Create<InstalledPackage, Unit>(p => {
                // this.RaisePropertyChanging(nameof(ObservableDatabase));
                // _database.RemovePackageSource(p.Id);
                // this.RaisePropertyChanged(nameof(ObservableDatabase));
                _observableDatabase.RemoveSource(p);
                return Unit.Default;
            });

            IncludeSystemPackages = false;
            OnlyWithSource = false;

            // _packagesCache = new SourceCache<InstalledPackage, string>(p => p.Id);
            // _packagesCache.AddOrUpdate(_database.Packages);
            // _filteredPackages = _packagesCache.Connect().ToProperty(this, x => x.FilteredPackages, out _filteredPackages);

            var packageFilterFunction = this
                .WhenAnyValue(x => x.FilterString, x => x.TypeFilterIndex, x => x.OnlyWithSource, x => x.IncludeSystemPackages, MakeFilter);

            _dynamicDataPackages = _observableDatabase.Connect()
                .Filter(packageFilterFunction)
                .Transform(p => new PackageViewModel(
                    p,
                    ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog(p.Id, p.PackageSource?.ToString() ?? "")),
                    RemovePackageSourceCommand,
                    _latestVersionCache        
                ))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _filteredPackages)
                .Subscribe();

            // return databasePackages.Where(p => (includeSystemPackages || p.IsCommunityPackage) &&
            //                                    (string.IsNullOrWhiteSpace(filterString) || 
            //                                     p.Id.Contains(filterString)) &&
            //                                    (filterTypeIndex == TYPE_FILTER_ALL_INDEX ||
            //                                     p.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
            //                                    (!onlyWithSource || p.PackageSource != null)
            //                         )
            //                         .Select(p => new PackageViewModel(
            //                             p,
            //                             ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog(p.Id, p.PackageSource?.ToString() ?? "")),
            //                             RemovePackageSourceCommand,
            //                             _latestVersionCache
            //                         ));

            // _filteredPackages = this.WhenAnyValue(x => x._observableDatabase.Packages, x => x.FilterString, x => x.TypeFilterIndex, x => x.OnlyWithSource, x => x.IncludeSystemPackages, 
            //                        FilterPackages
            //                     )
            //                     .ToProperty(this, x => x.FilteredPackages, out _filteredPackages);

            // this.WhenAnyValue(x => x.ObservableDatabase).Subscribe(p => Console.WriteLine("database changed!"));
            // this.WhenAnyValue(x => x.ObservableDatabase.Packages).Subscribe(p => Console.WriteLine("packages changed!"));
            // this.WhenAnyValue(x => x.FilteredPackages).Subscribe(p => Console.WriteLine("filtered packages changed!"));
            // this.PropertyChanged += (s, e) => Console.WriteLine($"--- property changed {e.PropertyName}");
            // _observableDatabase.PropertyChanged += (s, e) => Console.WriteLine($"This stupid property was changed: {e} ({s})!");
            // _observableDatabase.Packages.CollectionChanged += (s, e) => Console.WriteLine($"The colleciton was changed also {e} ({s})!");

            // _observableDatabase.AddPackageSource(new DummyPackageSource());
        }

        private async Task DoOpenAddPackageDialog(string packageId = "", string packageSourceString = "")
        {
            var dialog = new AddPackageViewModel(_database, _packageSourceRegistry, packageId, packageSourceString);
            var packageSource = await ShowAddPackageDialog.Handle(dialog);
            if (packageSource != null)
            {
                this.RaisePropertyChanging(nameof(ObservableDatabase));
                _observableDatabase.AddPackageSource(packageSource);
                // _database.AddPackageSource(packageSource.PackageId, packageSource);
                // Packages = new ObservableCollection<InstalledPackage>(database.Packages);
                this.RaisePropertyChanged(nameof(ObservableDatabase));
            }
        }

        private IEnumerable<PackageViewModel> FilterPackages(
            IEnumerable<InstalledPackage> databasePackages, string filterString, int filterTypeIndex, bool onlyWithSource, bool includeSystemPackages)
        {
            return databasePackages.Where(p => (includeSystemPackages || p.IsCommunityPackage) &&
                                               (string.IsNullOrWhiteSpace(filterString) || 
                                                p.Id.Contains(filterString)) &&
                                               (filterTypeIndex == TYPE_FILTER_ALL_INDEX ||
                                                p.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
                                               (!onlyWithSource || p.PackageSource != null)
                                    )
                                    .Select(p => new PackageViewModel(
                                        p,
                                        ReactiveCommand.CreateFromTask(async () => await DoOpenAddPackageDialog(p.Id, p.PackageSource?.ToString() ?? "")),
                                        RemovePackageSourceCommand,
                                        _latestVersionCache
                                    ));
        }

        private Func<InstalledPackage, bool> MakeFilter(string filterString, int filterTypeIndex, bool onlyWithSource, bool includeSystemPackages)
        {
            return p => 
                    (includeSystemPackages || p.IsCommunityPackage) &&
                    (string.IsNullOrWhiteSpace(filterString) || p.Id.Contains(filterString)) &&
                    (filterTypeIndex == TYPE_FILTER_ALL_INDEX ||
                     p.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
                    (!onlyWithSource || p.PackageSource != null);
        }

        public void ClearFilters()
        {
            FilterString = string.Empty;
            TypeFilterIndex = 0;
        }


    }
}
