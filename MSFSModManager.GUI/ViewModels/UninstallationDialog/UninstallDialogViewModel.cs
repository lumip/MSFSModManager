// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2022 Lukas <lumip> Prediger

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ReactiveUI;

using MSFSModManager.Core;
using System.Reactive;

namespace MSFSModManager.GUI.ViewModels
{
    class UninstallDialogViewModel : ViewModelBase
    {
        private ObservableDatabase _database;
        private readonly IEnumerable<InstalledPackage> _removalCandidates;
        public IEnumerable<string> RemovalCandidates => _removalCandidates.Select(p => p.Id);

        private ViewModelBase _content;
        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        public ReactiveCommand<Unit, Unit> UninstallCommand { get; }
        public ReactiveCommand<Unit, IEnumerable<string>> CloseCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private PackageUninstallationProgressViewModel? _uninstallationProgressList;
        public PackageUninstallationProgressViewModel? UninstallationProgressList
        {
            get => _uninstallationProgressList;
            set => this.RaiseAndSetIfChanged(ref _uninstallationProgressList, value);
        }

        private CancellationTokenSource _cancellationTokenSource;

        public UninstallDialogViewModel(ObservableDatabase database, IEnumerable<InstalledPackage> removalCandidates)
        {
            _database = database;
            _removalCandidates = removalCandidates;

            _cancellationTokenSource = new CancellationTokenSource();

            UninstallCommand = ReactiveCommand.CreateFromTask(
                async () => await Task.Factory.StartNew(() => DoUninstall(_cancellationTokenSource.Token))
            );

            CancelCommand = ReactiveCommand.Create(() => _cancellationTokenSource.Cancel());
            CloseCommand = ReactiveCommand.Create(() => {
                if (UninstallationProgressList != null)
                {
                    return UninstallationProgressList.UninstallingPackages
                                .Where(ipvm => ipvm.State == UninstallationState.Success)
                                .Select(ipvm => ipvm.Id);
                }
                return Enumerable.Empty<string>();
            });

            _content = new DependentPackagesLookupPageViewModel();

            Observable
                .FromAsync(async () => await Task.Run(() => DoResolveDependencies(_cancellationTokenSource.Token)))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe();
        }

        public void DoResolveDependencies(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            IEnumerable<DependencyNode> dependentPackages;
            try
            {
                GlobalLogger.Log(LogLevel.Info, "Looking up dependent packages for uninstallation...");
                dependentPackages = DependencyResolver.FindDependentPackages(_removalCandidates.Select(p => p.Id), _database.Database, ct);

                GlobalLogger.Log(LogLevel.Info, "Dependent package lookup complete.");
            }
            catch (AggregateException e)
            {
                Exception innerException = e.InnerException!;
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not look up dependent packages:");
                }
                GlobalLogger.Log(LogLevel.CriticalError, $"{innerException}");
                return;
            }
            catch (TaskCanceledException)
            {
                GlobalLogger.Log(LogLevel.Info, "Cancelled...");
                return;
            }
            catch (OperationCanceledException)
            {
                GlobalLogger.Log(LogLevel.Info, "Cancelled...");
                return;
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.Error, $"Unexpected error {e}");
                return;
            }

            UninstallationProgressList = new PackageUninstallationProgressViewModel(
                dependentPackages.Select(pdn => _database.GetInstalledPackage(pdn.PackageId).Package).Concat(_removalCandidates)
            );
            UninstallationProgressList.IsProgressVisible = false;

            Content = new UninstallPageViewModel();

        }

        public void DoUninstall(CancellationToken ct)
        {
            UninstallationProgressList!.IsProgressVisible = true;
            foreach (var pvm in UninstallationProgressList!.UninstallingPackages)
            {
                ct.ThrowIfCancellationRequested();

                pvm.TotalProgress = 100;
                pvm.CurrentProgress = 0;
                pvm.State = UninstallationState.Uninstalling;

                try
                {
                    _database.Uninstall(pvm.Package);
                    pvm.CurrentProgress = 100;
                    pvm.State = UninstallationState.Success;
                }
                catch (Exception e)
                {
                    GlobalLogger.Log(LogLevel.Error, $"Error while uninstalling {pvm.Id}: {e.Message}");
                    pvm.CurrentProgress = 0;
                    pvm.State = UninstallationState.Faulted;
                }


            }

            Content = new UninstallCompletedPageViewModel(
                UninstallationProgressList!.UninstallingPackages.Where(pvm => pvm.State == UninstallationState.Success),
                UninstallationProgressList!.UninstallingPackages.Where(pvm => pvm.State != UninstallationState.Success)
            );
        }
    }
}
