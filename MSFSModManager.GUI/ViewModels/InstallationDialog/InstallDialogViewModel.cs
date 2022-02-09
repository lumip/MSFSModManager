using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using ReactiveUI;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using System.Windows.Input;
using System.Reactive;

namespace MSFSModManager.GUI.ViewModels
{
    class InstallDialogViewModel : ViewModelBase
    {
        private ObservableDatabase _database;
        private readonly IEnumerable<InstalledPackage> _installationCandidates;
        public IEnumerable<PackageDependency> InstallationCandidates =>
            _installationCandidates.Select(p => new PackageDependency(
                p.Id,
                new VersionBounds(p.Version ?? VersionNumber.Zero, VersionNumber.Infinite)
            ));

        private ViewModelBase _content;
        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        private ObservableCollection<InstallationCandidateViewModel> _alreadyInstalledPackages;
        public ObservableCollection<InstallationCandidateViewModel> AlreadyInstalledPackages
        {
            get => _alreadyInstalledPackages;
            private set => this.RaiseAndSetIfChanged(ref _alreadyInstalledPackages, value);
        }

        private IEnumerable<PackageManifest>? _toInstallPackages;

        private DependencyResolutionPageViewModel _dependencyResolutionView;

        public ReactiveCommand<Unit, Unit> InstallCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        private PackageInstallationProgressViewModel? _installationProgressList;
        public PackageInstallationProgressViewModel? InstallationProgressList
        {
            get => _installationProgressList;
            set => this.RaiseAndSetIfChanged(ref _installationProgressList, value);
        }

        private IVersionNumber _gameVersion;

        private CancellationTokenSource _cancellationTokenSource;

        public InstallDialogViewModel(ObservableDatabase database, IEnumerable<InstalledPackage> installationCandidates, IVersionNumber gameVersion)
        {
            _database = database;
            _gameVersion = gameVersion;
            _installationCandidates = installationCandidates;
            _alreadyInstalledPackages = new ObservableCollection<InstallationCandidateViewModel>();
            _toInstallPackages = null;

            _installationProgressList = null;
            _cancellationTokenSource = new CancellationTokenSource();


            InstallCommand = ReactiveCommand.CreateFromTask(DoInstall);

            CancelCommand = ReactiveCommand.Create(() => _cancellationTokenSource.Cancel());
            CloseCommand = ReactiveCommand.Create(() => Unit.Default);

            _dependencyResolutionView = new DependencyResolutionPageViewModel();
            _content = _dependencyResolutionView;

            Observable.FromAsync(() => DoResolveDependencies(_cancellationTokenSource.Token)).ObserveOn(RxApp.MainThreadScheduler).Subscribe();
        }

        public async Task DoResolveDependencies(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            // Add decorators for hidden "fs-base" packages that are not captured by PackageDatabase
            IPackageDatabase decoratedDatabase = new HiddenBasePackagesDatabase(_database.Database);
            IPackageSourceRepository sourceRepository = new HiddenBasePackageSourceRepositoryDecorator(
                new PackageDatabaseSource(decoratedDatabase)
            );

            var monitor = new MultiProgressMonitor();
            monitor.Add(new LogProgressMonitor());
            monitor.Add(_dependencyResolutionView);

            IEnumerable<PackageManifest> candidatesAndDependencies;
            try
            {
                GlobalLogger.Log(LogLevel.Info, "Resolving package dependencies...");
                candidatesAndDependencies = (await DependencyResolver.ResolveDependencies(
                        InstallationCandidates,
                        sourceRepository,
                        _gameVersion,
                        monitor,
                        ct)
                    );
                GlobalLogger.Log(LogLevel.Info, "Dependency resolution complete.");
            }
            catch (AggregateException e)
            {
                Exception innerException = e.InnerException!;
                if (innerException is PackageNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not resolve dependencies: A source of a required package could not be found.");
                }
                else if (innerException is VersionNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not resolve dependencies: A suitable package version for a required package could not be found.");
                }
                else
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not resolve dependencies: Unknown error.");
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

            AlreadyInstalledPackages = new ObservableCollection<InstallationCandidateViewModel>(
                candidatesAndDependencies
                    .Where(m => decoratedDatabase.Contains(m.Id, new VersionBounds(m.Version)))
                    .Select(m => new InstallationCandidateViewModel(m.Id, m.Version.ToString()))
            );

            ct.ThrowIfCancellationRequested();

            _toInstallPackages = candidatesAndDependencies.Where(m => !decoratedDatabase.Contains(m.Id, new VersionBounds(m.Version)));

            InstallationProgressList = new PackageInstallationProgressViewModel(_toInstallPackages);
            InstallationProgressList.IsProgressVisible = false;

            Content = new InstallPageViewModel();
        }

        public async Task DoInstall()
        {
            InstallationProgressList!.IsProgressVisible = true;
            IPackageSourceRepository sourceRepository = new PackageDatabaseSource(_database.Database);

            List<Task> installationTasks = new List<Task>(_toInstallPackages!.Count());

            GlobalLogger.Log(LogLevel.Info, "Installing packages:");
            foreach (var package in _toInstallPackages!)
            {
                GlobalLogger.Log(LogLevel.Info, $"{package.Id,-60} {package.Version,14}");

                IPackageInstaller installer = sourceRepository.GetSource(package.Id).GetInstaller(package.SourceVersion);
                Task installationTask = Task.Run(
                    async () => await _database.InstallPackage(
                        installer, (IProgressMonitor)InstallationProgressList!, _cancellationTokenSource.Token
                    )
                );
                installationTasks.Add(installationTask);
                InstallationProgressList!.SetInstallationTask(package.Id, installationTask);
            }

            try
            {
                await Task.WhenAll(installationTasks);
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.Error, $"Error while installing package: {e.Message}");
            }

            // todo: need to clean/update GUI state!
        }
    }
}
