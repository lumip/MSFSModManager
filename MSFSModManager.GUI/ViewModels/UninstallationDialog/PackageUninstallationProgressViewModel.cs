// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using Avalonia;
using ReactiveUI;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;

using MSFSModManager.Core;
using System;

namespace MSFSModManager.GUI.ViewModels
{
    class PackageUninstallationProgressViewModel : ViewModelBase
    {

        private SourceCache<UninstallingPackageViewModel, string> _packagesToRemove;

        private IDisposable _dynamicData;
        private ReadOnlyObservableCollection<UninstallingPackageViewModel> _uninstallingPackages;
        public ReadOnlyObservableCollection<UninstallingPackageViewModel> UninstallingPackages => _uninstallingPackages;

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set => this.RaiseAndSetIfChanged(ref _isProgressVisible, value);
        }

        public PackageUninstallationProgressViewModel(IEnumerable<InstalledPackage> packagesToRemove)
        {
            _isProgressVisible = true;
            
            _packagesToRemove = new SourceCache<UninstallingPackageViewModel, string>(p => p.Id);
            _packagesToRemove.AddOrUpdate(packagesToRemove.Select(m => new UninstallingPackageViewModel(m)));

            _dynamicData = _packagesToRemove
                                        .Connect()
                                        .ObserveOn(RxApp.MainThreadScheduler)
                                        .Bind(out _uninstallingPackages)
                                        .Subscribe();

        }

    }
}
