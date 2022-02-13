// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021,2022 Lukas <lumip> Prediger

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

using MSFSModManager.GUI.Settings;
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
            
            var settingsBuilder = UserSettingsBuilder.LoadFromConfigFile();
            if (!settingsBuilder.IsComplete)
            {
                // todo: WIP currently this just opens the dialog and then quits the application without storing the new settings
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop_)
                {
                    desktop_.MainWindow = new SettingsView
                    {
                        DataContext = new SettingsViewModel(settingsBuilder)
                    };

                    base.OnFrameworkInitializationCompleted();

                    return;
                }
            }
            var settings = settingsBuilder.Build();
            Console.WriteLine(settings.ContentPath);
            
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
            PackageDatabase database = new PackageDatabase(settings.ContentPath, sourceRegistry);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(database, sourceRegistry, gameVersion, logger, settings.ContentPath),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

    }
}