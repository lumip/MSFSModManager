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
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                LogViewModel logger = new LogViewModel();
                GlobalLogger.Instance = logger;

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
                
                var settingsBuilder = UserSettingsBuilder.LoadFromConfigFile();
                if (!settingsBuilder.IsComplete)
                {
                    var settingsView = new SettingsView();
                    var settingsViewModel = new SettingsViewModel(settingsBuilder);
                    settingsView.DataContext = settingsViewModel;
                    settingsViewModel.ApplyCommand.Subscribe(settings => {
                        if (settings != null)
                        {
                            settings.Save();
                            ShowMainWindow(settings, desktop, logger);
                        }
                        else
                        {
                            ((ILogger)logger).Log(LogLevel.CriticalError, "Aborting: No settings configured.");
                            logger.DumpToConsole();
                        }
                    });
                    settingsViewModel.CancelCommand.Subscribe(settings => {
                        ((ILogger)logger).Log(LogLevel.CriticalError, "Aborting: No settings configured.");
                        logger.DumpToConsole();
                    });
                    settingsView.Show();
                }
                else
                {
                    var settings = settingsBuilder.Build();
                    ShowMainWindow(settings, desktop, logger);
                }
            }
            base.OnFrameworkInitializationCompleted();
        }


        private void ShowMainWindow(UserSettings settings, IClassicDesktopStyleApplicationLifetime desktop, LogViewModel logger)
        {
            
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

            PackageVersionCache versionCache = new PackageVersionCache();

            Avalonia.Controls.Window? window = null;
            if (desktop.MainWindow != null)
            {
                window = desktop.MainWindow;
            }

            desktop.MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(settings, sourceRegistry, gameVersion, logger, versionCache),
            };

            desktop.MainWindow.Show();

        }
    }
}
