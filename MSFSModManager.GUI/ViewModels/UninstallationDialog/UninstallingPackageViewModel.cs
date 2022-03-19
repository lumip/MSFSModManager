// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using ReactiveUI;

using MSFSModManager.Core;
using Avalonia.Media;
using System.Reactive.Linq;

namespace MSFSModManager.GUI.ViewModels
{
    enum UninstallationState
    {
        Pending,
        Uninstalling,
        Faulted,
        Success
    }

    class UninstallingPackageViewModel : ReactiveObject
    {
        public string Id => Package.Id;

        public string DependingOnId { get; }

        private readonly ObservableAsPropertyHelper<IBrush> _statusLabelColor;
        public IBrush StatusLabelColor => _statusLabelColor.Value;

        private readonly ObservableAsPropertyHelper<FontWeight> _statusLabelFontWeight;
        public FontWeight StatusLabelFontWeight => _statusLabelFontWeight.Value;

        private UninstallationState _state;
        public UninstallationState State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }

        private readonly ObservableAsPropertyHelper<string> _statusLabel;
        public string StatusLabel => _statusLabel.Value;

        private readonly ObservableAsPropertyHelper<bool> _isIndeterminate;
        public bool IsIndeterminate => _isIndeterminate.Value;

        private long _totalProgress;
        public long TotalProgress
        {
            get => _totalProgress;
            set => this.RaiseAndSetIfChanged(ref _totalProgress, value);
        }

        private long _currentProgress;
        public long CurrentProgress
        {
            get => _currentProgress;
            set => this.RaiseAndSetIfChanged(ref _currentProgress, value);
        }

        public InstalledPackage Package { get; }

        public UninstallingPackageViewModel(InstalledPackage package, string? dependingOn = null)
        {
            Package = package;
            DependingOnId = dependingOn != null ? dependingOn : "";

            _isIndeterminate = this
                .WhenAnyValue(x => x.State)
                .Select(s => s == UninstallationState.Pending || s == UninstallationState.Uninstalling)
                .ToProperty(this, x => x.IsIndeterminate, out _isIndeterminate);
            
            _statusLabel = this
                .WhenAnyValue(x => x.State)
                .Select(s => {
                    switch (s)
                    {
                        case UninstallationState.Pending: return "Pending";
                        case UninstallationState.Uninstalling: return "Uninstalling";
                        case UninstallationState.Success: return "Completed!";
                        case UninstallationState.Faulted: return "Error!";
                        default: return "Pending";
                    }
                })
                .ToProperty(this, x => x.StatusLabel, out _statusLabel);
            _totalProgress = 0;
            _currentProgress = 0;
            _state = UninstallationState.Pending;
            _statusLabelColor = this
                .WhenAnyValue(x => x.State)
                .Select(s => {
                    switch (s)
                    {
                        case UninstallationState.Faulted: return Brushes.Red;
                        default: return Brushes.Black;
                    }
                })
                .ToProperty(this, x => x.StatusLabelColor, out _statusLabelColor);
            _statusLabelFontWeight = this
                .WhenAnyValue(x => x.State)
                .Select(s => {
                    switch (s)
                    {
                        case UninstallationState.Faulted:
                        case UninstallationState.Success:
                            return FontWeight.Bold;
                        default: return FontWeight.Normal;
                    }
                })
                .ToProperty(this, x => x.StatusLabelFontWeight, out _statusLabelFontWeight);
        }

    }
}
