// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

using MSFSModManager.Core;
using MSFSModManager.Core.PackageSources;
using MSFSModManager.Core.PackageSources.Github;

namespace MSFSModManager.CLI
{
    class Program
    {

        static ConsoleRenderer renderer = new ConsoleRenderer();

        static IVersionNumber gameVersion = VersionNumber.Infinite;

        enum ReturnCode
        {
            Success = 0,
            ModeError = 1,
            ArgumentError = 2,
            NoPackageSource = 3,
            UnknownError = 4
        }

        static (Dictionary<string, string>, HashSet<string>) ParseArguments(string[] args)
        {
            HashSet<string> optionsWithValue = new HashSet<string>() {
                "contentPath", "filterType"
            };
            Dictionary<string, string> options = new Dictionary<string, string>();
            HashSet<string> flags = new HashSet<string>();
            for (int i = 1; i < args.Length; ++i)
            {
                if (args[i].StartsWith("--"))
                {
                    string option = args[i].Substring(2);
                    if (optionsWithValue.Contains(option))
                    {
                        if ((i + 1) >= args.Length || args[i + 1].StartsWith("--"))
                        {
                            throw new Exception($"Command line option {option} requires a value!");
                        }
                        string value = args[i + 1];
                        options.Add(option, value);
                    }
                    else
                    {
                        flags.Add(option);
                    }
                }
            }
            return (options, flags);
        }

        static int Main(string[] args)
        {
            GlobalLogger.Instance = new ConsoleLogger(renderer);

            if (args.Length == 0)
            {
                GlobalLogger.Log(LogLevel.CriticalError, "Missing mode command!");
                return 1;
            }
            string command = args[0];
            Dictionary<string, string> options;
            HashSet<string> flags;
            try
            {
                (options, flags) = ParseArguments(args);
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.CriticalError, e.Message);
                return 1;
            }

            string contentPath;
            if (options.ContainsKey("contentPath"))
            {
                contentPath = options["contentPath"];
            }
            else
            {
                try
                {
                    contentPath = ConfigReader.ReadContentPathFromDefaultLocations();
                }
                catch (FileNotFoundException)
                {
                    // development fallback!
                    contentPath = ConfigReader.ReadContentPathFromConfig(@"/media/data/MSFSData/UserCfg.opt");
                }
            }
            
            try
            {
                gameVersion = new RegistryVersionDetector().Version;
                GlobalLogger.Log(LogLevel.Info, $"Game version {gameVersion}.");
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.Error, "Could not detect game version, assuming latest! This is caused by error:");
                GlobalLogger.Log(LogLevel.Error, $"{e}");
            }

            
            GlobalLogger.Log(LogLevel.Info, $"Content directory: {contentPath}");


            try
            {
                switch (command)
                {
                    case "list":
                        return (int)ListInstalled(contentPath, options, flags);
                    case "add-source":
                        return (int)AddSource(contentPath, args);
                    case "remove-source":
                        return (int)RemoveSource(contentPath, args);
                    case "install":
                        return (int)Install(contentPath, args);
                    case "uninstall":
                        return (int)Uninstall(contentPath, args);
                    case "show-available":
                        return (int)ShowAvailable(contentPath, args);
                    case "update":
                        return (int)Update(contentPath, args);
                    case "export":
                        return (int)Export(contentPath, args);
                    default:
                        GlobalLogger.Log(LogLevel.CriticalError, $"Unknown mode command {command}.");
                        return (int)ReturnCode.ModeError;
                }
            }
            catch (Exception e)
            {
                GlobalLogger.Log(LogLevel.CriticalError, e.ToString());
                return (int)ReturnCode.UnknownError;
            }
        }

        static ReturnCode ListInstalled(string contentPath, Dictionary<string, string> options, HashSet<string> flags)
        {
            bool includeOfficial = flags.Contains("includeOfficial");
            string? filterType = null;
            if (options.ContainsKey("filterType"))
                filterType = options["filterType"];
            return ListInstalled(contentPath, includeOfficial, filterType);
        }

        static (PackageDatabase, PackageCache) LoadDatabase(string contentPath)
        {
            HttpClient client = new HttpClient();
            PackageCache cache = new PackageCache(Path.Join(Path.GetTempPath(), "msfsmodmanager_cache"));
            PackageSourceRegistry sourceRegistry = new PackageSourceRegistry(cache, client);
            PackageDatabase database = new PackageDatabase(contentPath, sourceRegistry);
            return (database, cache);
        }

        static ReturnCode ListInstalled(string contentPath, bool includeOfficial, string? filterType)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            IEnumerable<InstalledPackage> packages = includeOfficial ? database.Packages : database.CommunityPackages;
            GlobalLogger.Log(LogLevel.Output, String.Format("{0,-12}  {1,-60}{2,14}    {3}", "Content Type", "Package Id", "Version", "Source"));
            GlobalLogger.Log(LogLevel.Output, "##########################################################################################");
            foreach (var packagesOfType in packages.GroupBy(package => package.Type).Where(grouping => (filterType != null) ? grouping.Key.ToLowerInvariant() == filterType.ToLowerInvariant() : true))
            {
                foreach (InstalledPackage package in packagesOfType)
                {
                    IPackageSource? source = package.PackageSource;
                    PackageManifest? manifest = package.Manifest;
                    string sourceString = (source == null) ? "" : source.ToString()!;
                    string typeString = "UNKNOWN";
                    string versionString = "not installed";
                    if (manifest != null)
                    {
                        typeString = manifest.Type;
                        versionString = manifest.Version.ToString();
                    }
                    GlobalLogger.Log(LogLevel.Output, $"{typeString,-12}  {package.Id,-60}{versionString,14}   {sourceString}");
                }
            }
            return ReturnCode.Success;
        }

        static ReturnCode AddSource(string contentPath, string[] args)
        {
            string packageId = args[1];
            string[] sourceStrings = args.Skip(2).ToArray();
            return AddSource(contentPath, packageId, sourceStrings);
        }

        static ReturnCode AddSource(string contentPath, string packageId, string[] sourceStrings)
        {
            (PackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            IPackageSource source = database.SourceRegistry.ParseSourceStrings(packageId, sourceStrings);

            database.AddPackageSource(packageId, source);
            return ReturnCode.Success;
        }

        static ReturnCode RemoveSource(string contentPath, string[] args)
        {
            string packageId = args[1];
            return RemoveSource(contentPath, packageId);
        }

        static ReturnCode RemoveSource(string contentPath, string packageId)
        {
            (PackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            database.RemovePackageSource(packageId);
            return ReturnCode.Success;
        }

        static ReturnCode Install(string contentPath, string[] args)
        {
            string packageId = args[1];
            if (args.Length > 2)
            {
                string[] sourceStrings = args.Skip(2).ToArray();
                return InstallFromGivenSource(contentPath, packageId, sourceStrings);
            }
            return InstallFromKnownSource(contentPath, packageId);
        }

        static ConsoleStatusLines statusLines = new ConsoleStatusLines(renderer);

        class ConsoleProgressMonitor : IProgressMonitor
        {
            public void DownloadStarted(IDownloadProgressMonitor monitor)
            {
                ConsoleRenderer.LineHandle line = statusLines.GetLineHandle(monitor.PackageId);

                ProgressBar bar = new ProgressBar($"downloading {monitor.PackageId} {monitor.Version}", $"{monitor.TotalSize / (1024*1024)} MB", line);
                bar.Render();
                monitor.UserData = bar;
                monitor.DownloadProgress += OnDownloadProgress;
            }

            void OnDownloadProgress(IDownloadProgressMonitor monitor)
            {
                ProgressBar bar = (ProgressBar)monitor.UserData!;
            
                bar.Update(monitor.CurrentPercentage);
                bar.Render();
            }

            public void ExtractionCompleted(string packageId, IVersionNumber versionNumber)
            {
                ConsoleRenderer.LineHandle line = statusLines.GetLineHandle(packageId);
                
                line.Clear();
                line.Write($"Extracting {packageId} {versionNumber} completed.");
            }

            public void ExtractionStarted(string packageId, IVersionNumber versionNumber)
            {
                ConsoleRenderer.LineHandle line = statusLines.GetLineHandle(packageId);

                line.Clear();
                line.Write($"Extracting {packageId} {versionNumber} ...");
            }
        }

        static ReturnCode InstallFromKnownSource(string contentPath, string packageId)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            database = new HiddenBasePackagesDatabase(database);

            GlobalLogger.Log(LogLevel.Info, "Resolving package dependencies:");

            PackageDependency[] installationCandidates = new PackageDependency[] {
                new PackageDependency(packageId, VersionBounds.Unbounded)
            };

            IPackageSourceRepository source = new HiddenBasePackageSourceRepositoryDecorator(new PackageDatabaseSource(database));
            var monitor = new ConsoleProgressMonitor();

            IEnumerable<PackageManifest> toInstall;
            try
            {
                toInstall = DependencyResolver.ResolveDependencies(installationCandidates, source, gameVersion, monitor).Result
                    .Where(m => !database.Contains(m.Id, new VersionBounds(m.SourceVersion)));
            }
            catch (AggregateException e)
            {
                Exception innerException = e.InnerException!;
                if (innerException is PackageNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A source of a required package could not be found.");
                }
                else if (innerException is VersionNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A suitable package version for a required package could not be found.");
                }
                else
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: Unknown error.");
                }
                GlobalLogger.Log(LogLevel.CriticalError, $"{innerException}");
                return ReturnCode.UnknownError;
            }
            

            GlobalLogger.Log(LogLevel.Info, "Installing packages:");
            foreach (var package in toInstall)
            {
                GlobalLogger.Log(LogLevel.Info, $"{package.Id,-60} {package.Version,14}");

                IPackageInstaller installer = source.GetSource(package.Id).GetInstaller(package.SourceVersion);
                
                database.InstallPackage(installer, monitor).Wait();
            }

            return ReturnCode.Success;
        }

        static ReturnCode InstallFromGivenSource(string contentPath, string packageId, string[] sourceStrings)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            IPackageSource source = ((PackageDatabase)database).SourceRegistry.ParseSourceStrings(packageId, sourceStrings);
            GlobalLogger.Log(LogLevel.Info, $"Installing {packageId} from source {source} without storing source for future use!");

            database.AddPackageSource(packageId, source);
            try
            {
                InstallFromKnownSource(contentPath, packageId);
            }
            finally
            {
                database.RemovePackageSource(packageId);
            }

            return ReturnCode.Success;
        }

        static ReturnCode Uninstall(string contentPath, string[] args)
        {
            string packageId = args[1];
            return Uninstall(contentPath, packageId);
        }

        static ReturnCode Uninstall(string contentPath, string packageId)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);
            database.Uninstall(packageId);
            return ReturnCode.Success;
        }

        static ReturnCode ShowAvailable(string contentPath, string[] args)
        {
            string packageId = args[1];
            return ShowAvailable(contentPath, packageId);
        }

        static ReturnCode ShowAvailable(string contentPath, string packageId)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            IPackageSource? source = database.GetInstalledPackage(packageId).PackageSource;
            if (source == null)
            {
                GlobalLogger.Log(LogLevel.CriticalError, $"No source for {packageId} known.");
                return ReturnCode.NoPackageSource;
            }

            foreach (var version in source.ListAvailableVersions().Result)
            {
                GlobalLogger.Log(LogLevel.Output, $"{version}");
            }

            return ReturnCode.Success;
        }

        static ReturnCode Update(string contentPath, string[] args)
        {
            return Update(contentPath);
        }

        static ReturnCode Update(string contentPath)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);

            database = new HiddenBasePackagesDatabase(database);

            GlobalLogger.Log(LogLevel.Info, "Resolving package dependencies:");

            IEnumerable<PackageDependency> installationCandidates = database.CommunityPackages
                .Where(p => p.PackageSource != null)
                .Select(p => new PackageDependency(
                                    p.Id,
                                    new VersionBounds(
                                            (p.Manifest == null) ? VersionNumber.Zero : p.Manifest.Version,
                                            VersionNumber.Infinite
                                    )
                            )
                );

            IPackageSourceRepository source = new HiddenBasePackageSourceRepositoryDecorator(new PackageDatabaseSource(database));

            var monitor = new ConsoleProgressMonitor();

            IEnumerable<PackageManifest> toInstall;
            try
            {
                toInstall = DependencyResolver.ResolveDependencies(installationCandidates, source, gameVersion, monitor).Result
                    .Where(m => !database.Contains(m.Id, new VersionBounds(m.SourceVersion)));
            }
            catch (AggregateException e)
            {
                Exception innerException = e.InnerException!;
                if (innerException is PackageNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A source of a required package could not be found.");
                }
                else if (innerException is VersionNotAvailableException)
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: A suitable package version for a required package could not be found.");
                }
                else
                {
                    GlobalLogger.Log(LogLevel.CriticalError, $"Could not complete installation: Unknown error.");
                }
                GlobalLogger.Log(LogLevel.CriticalError, $"{innerException}");
                return ReturnCode.UnknownError;
            }

            GlobalLogger.Log(LogLevel.Info, "Installing packages:");
            foreach (var package in toInstall)
            {
                GlobalLogger.Log(LogLevel.Info, $"{package.Id,-60} {package.Version,14}");
                database.InstallPackage(source.GetSource(package.Id).GetInstaller(package.SourceVersion), monitor).Wait();
            }

            return ReturnCode.Success;
        }

        static ReturnCode Export(string contentPath, string[] args)
        {
            bool onlyWithSources = args.Contains("--onlyWithSources");
            bool ignoreVersion = args.Contains("--ignoreVersion");
            return Export(contentPath, onlyWithSources, ignoreVersion);
        }

        static ReturnCode Export(string contentPath, bool onlyWithSources, bool ignoreVersion)
        {
            (IPackageDatabase database, PackageCache cache) = LoadDatabase(contentPath);
            
            GlobalLogger.Log(LogLevel.Output, PackageDatabaseExporter.SerializeDatabaseExport(database, onlyWithSources, ignoreVersion));

            return ReturnCode.Success;
        }
    }
}
