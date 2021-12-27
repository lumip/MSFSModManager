// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ReactiveUI;
using System.Reactive.Linq;

using MSFSModManager.Core;

namespace MSFSModManager.GUI.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {

        private PackageDatabase _database;

        private readonly ObservableAsPropertyHelper<IEnumerable<PackageViewModel>> _packages;
        public IEnumerable<PackageViewModel> Packages => _packages.Value;

        public LogViewModel Log { get; }

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


        public MainWindowViewModel(PackageDatabase database, LogViewModel log)
        {
            _database = database;
            Log = log;

            _filterString = string.Empty;

            var typesInDatabase = _database.Packages.Select(p => p.Type).ToHashSet();
            var types = new List<string>();
            types.Add(TYPE_FILTER_ALL_STRING);
            types.AddRange(typesInDatabase);
            PackageTypes = types;

            IncludeSystemPackages = false;
            OnlyWithSource = false;
            
            _packages = this.WhenAnyValue(x => x.FilterString, x => x.TypeFilterIndex, x => x.OnlyWithSource, x => x.IncludeSystemPackages, 
                                FilterPackages
                            )
                            .ToProperty(this, x => x.Packages, out _packages);
        }

        private IEnumerable<PackageViewModel> FilterPackages(
            string filterString, int filterTypeIndex, bool onlyWithSource, bool includeSystemPackages)
        {
            IEnumerable<InstalledPackage> databasePackages = includeSystemPackages ? this._database.Packages : this._database.CommunityPackages;
            return databasePackages.Where(p => (string.IsNullOrWhiteSpace(filterString) || 
                                                p.Id.Contains(filterString)) &&
                                               (filterTypeIndex == TYPE_FILTER_ALL_INDEX ||
                                                p.Type.ToLowerInvariant().Equals(PackageTypes[filterTypeIndex].ToLowerInvariant())) &&
                                               (!onlyWithSource || p.PackageSource != null)
                                    )
                                    .Select(p => new PackageViewModel(p));
        }

        public void ClearFilters()
        {
            FilterString = string.Empty;
            TypeFilterIndex = 0;
        }


    }
}
