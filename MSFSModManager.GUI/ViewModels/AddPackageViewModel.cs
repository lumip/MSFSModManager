// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Windows.Input;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using Avalonia.Media;

namespace MSFSModManager.GUI.ViewModels
{

    class AddPackageDialogReturnValues
    {
        public IPackageSource? PackageSource;
        public bool MarkForInstallation;
        public bool InstallAfterAdding;

        public AddPackageDialogReturnValues(IPackageSource? packageSource, bool markForInstallation, bool installAfterAdding)
        {
            PackageSource = packageSource;
            MarkForInstallation = markForInstallation;
            InstallAfterAdding = installAfterAdding;
        }
    }

    class AddPackageViewModel : ViewModelBase
    {


        private IPackageSourceRegistry _packageSourceRegistry;


# region Direct UI properties
        private string _packageSourceString;
        public string PackageSourceString
        {
            get => _packageSourceString;
            set => this.RaiseAndSetIfChanged(ref _packageSourceString, value);
        }

        private string _packageId;
        public string PackageId
        {
            get => _packageId;
            set => this.RaiseAndSetIfChanged(ref _packageId, value);
        }

        private bool _markForInstallation;
        public bool MarkForInstallation
        {
            get => _markForInstallation;
            set => this.RaiseAndSetIfChanged(ref _markForInstallation, value);
        }

        private bool _installAfterAdding;
        public bool InstallAfterAdding
        {
            get => _installAfterAdding;
            set => this.RaiseAndSetIfChanged(ref _installAfterAdding, value);
        }

#endregion

#region Derived UI Properties
        private readonly ObservableAsPropertyHelper<IPackageSource?> _packageSource;
        public IPackageSource? PackageSource => _packageSource.Value;

        private readonly ObservableAsPropertyHelper<IBrush> _packageIdTextColor;
        public IBrush PackageIdTextColor => _packageIdTextColor.Value;

        private readonly ObservableAsPropertyHelper<string> _addButtonLabel;
        public string AddButtonLabel => _addButtonLabel.Value;
#endregion


#region UI Commands

        public ReactiveCommand<Unit, AddPackageDialogReturnValues> AddPackageCommand { get; }
        public ReactiveCommand<Unit, AddPackageDialogReturnValues> CancelCommand { get; }

#endregion

        public AddPackageViewModel(ObservableDatabase database, IPackageSourceRegistry packageSourceRegistry, string packageId = "", string packageSourceString = "")
        {
            _packageSourceRegistry = packageSourceRegistry;
            _packageSourceString = packageSourceString;
            _packageId = packageId;

            _packageIdTextColor = this
                .WhenAnyValue(x => x.PackageId, id => database.Contains(id) ? Brushes.Orange : Brushes.Black)
                .ToProperty(this, x => x.PackageIdTextColor, out _packageIdTextColor);
            _addButtonLabel = this
                .WhenAnyValue(x => x.PackageId, id => database.Contains(id) ? "Update" : "Add")
                .ToProperty(this, x => x.AddButtonLabel, out _addButtonLabel);

            _packageSource = this
                .WhenAnyValue(x => x.PackageSourceString)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .SelectMany(ParsePackageSourceString)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToProperty(this, x => x.PackageSource, out _packageSource);

            var isValidSource = this
                .WhenAnyValue(x => x.PackageSource)
                .Select(source => source != null);

            this.WhenAnyValue(x => x.PackageSource).Subscribe(packageSource => {
                if (packageSource != null)
                {
                    PackageId = packageSource.PackageId;
                }
            });

            AddPackageCommand = ReactiveCommand.Create(
                () => new AddPackageDialogReturnValues(PackageSource, MarkForInstallation, InstallAfterAdding),
                isValidSource
            );
            CancelCommand = ReactiveCommand.Create(
                () => new AddPackageDialogReturnValues(null, MarkForInstallation, InstallAfterAdding)
            );
        }

        private async Task<IPackageSource?> ParsePackageSourceString(string combinedPackageSourceString, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(combinedPackageSourceString)) return null;
            string[] packageSourceStrings = combinedPackageSourceString.Trim().Split(' ');

            if (!_packageSourceRegistry.IsWellFormedSourceString(packageSourceStrings)) return null;

            try
            {
                IPackageSource res = await _packageSourceRegistry.ParseSourceStrings(packageSourceStrings, ct);
                return res;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
