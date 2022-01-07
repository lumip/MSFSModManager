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

            IVersionNumber gameVersion = VersionNumber.Infinite;
            try
            {
                gameVersion = new RegistryVersionDetector().Version;
                GlobalLogger.Log(LogLevel.Info, $"Game version {gameVersion}.");
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.Error, "Could not detect game version, assuming latest! This is caused by error:");
                GlobalLogger.Log(LogLevel.Error, $"{e.Message}");
            }

            HttpClient client = new HttpClient();
            PackageCache cache = new PackageCache(Path.Join(Path.GetTempPath(), "msfsmodmanager_cache"));
            PackageSourceRegistry sourceRegistry = new PackageSourceRegistry(cache, client);
            PackageDatabase database = new PackageDatabase(contentPath, sourceRegistry);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(database, sourceRegistry, gameVersion, logger, contentPath),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}