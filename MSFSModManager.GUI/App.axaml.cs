// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MSFSModManager.GUI.ViewModels;
using MSFSModManager.GUI.Views;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using System.Net.Http;
using System.IO;

using System.Collections.Generic;
using System.Linq;

using System.Threading.Tasks;

namespace MSFSModManager.GUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            LogViewModel logger = new LogViewModel();
            GlobalLogger.Instance = logger; 

            GlobalLogger.Log(LogLevel.CriticalError, "Log initialized!");

            string contentPath;
            try
            {
                contentPath = ConfigReader.ReadContentPathFromDefaultLocations();
            }
            catch (FileNotFoundException)
            {
                // development fallback!
                contentPath = ConfigReader.ReadContentPathFromConfig(@"/media/data/MSFSData/UserCfg.opt");
            }

            HttpClient client = new HttpClient();
            PackageCache cache = new PackageCache(Path.Join(Path.GetTempPath(), "msfsmodmanager_cache"));
            PackageSourceRegistry sourceRegistry = new PackageSourceRegistry(cache, client);
            PackageDatabase database = new PackageDatabase(contentPath, sourceRegistry);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(database, logger),
                };
            }

            base.OnFrameworkInitializationCompleted();

            // Task.Run(() => CheckForUpdates(database, VersionNumber.Infinite));
        }

        // private void CheckForUpdates(IPackageDatabase database, IVersionNumber gameVersion)
        // {
            

        //     database = new HiddenBasePackagesDatabase(database);

        //     GlobalLogger.Log(LogLevel.Info, "Resolving package dependencies:");

        //     IEnumerable<PackageDependency> installationCandidates = database.CommunityPackages
        //         .Where(p => p.PackageSource != null)
        //         .Select(p => new PackageDependency(
        //                             p.Id,
        //                             new VersionBounds(
        //                                     (p.Manifest == null) ? VersionNumber.Zero : p.Manifest.Version,
        //                                     VersionNumber.Infinite
        //                             )
        //                     )
        //         );

                
        //     IPackageSourceRepository source = new HiddenBasePackageSourceRepositoryDecorator(new PackageDatabaseSource(database));


        //     IEnumerable<PackageManifest> toInstall;
        //     try
        //     {
        //         toInstall = DependencyResolver.ResolveDependencies(installationCandidates, source, gameVersion).Result
        //             .Where(m => !database.Contains(m.Id, new VersionBounds(m.SourceVersion)));
        //     }
        //     catch (AggregateException e)
        //     {
        //         Exception innerException = e.InnerException!;
        //         if (innerException is PackageNotAvailableException)
        //         {
        //             GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A source of a required package could not be found.");
        //         }
        //         else if (innerException is VersionNotAvailableException)
        //         {
        //             GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A suitable package version for a required package could not be found.");
        //         }
        //         else
        //         {
        //             GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: Unknown error.");
        //         }
        //         GlobalLogger.Log(LogLevel.CriticalError, $"{innerException}");
        //     }

            

        // }
    }
}