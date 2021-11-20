// SPDX-License-Identifier: GPL-3.0-or-later
// Copyright 2021 Lukas <lumip> Prediger

using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MSFSModManager.Core
{

    public class ConfigParsingException : Exception
    {

    }

    public class ConfigReader
    {

        public static string ReadContentPathFromDefaultLocations()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            string microsoftStoreConfigPath = Path.Join(
                localAppDataPath, "Packages", "Microsoft.FlightSimulator_8wekyb3d8bbwe"
            );
            microsoftStoreConfigPath = Path.Join(microsoftStoreConfigPath, "LocalCache", "UserCfg.opt");

            string steamConfigPath = Path.Join(
                appDataPath, "Microsoft Flight Simulator", "UserCfg.opt"
            );

            if (File.Exists(microsoftStoreConfigPath))
            {
                return ReadContentPathFromConfig(microsoftStoreConfigPath);
            }
            else if (File.Exists(steamConfigPath))
            {
                return ReadContentPathFromConfig(steamConfigPath);
            }
            throw new FileNotFoundException("No config file could be found at any of the default paths.");
        }

        public static string ReadContentPathFromConfig(string configFilePath)
        {
            string[] optFileLines = File.ReadAllLines(configFilePath);
            Regex re = new Regex("InstalledPackagesPath \"(?<Path>.*)\"");

            foreach (string line in optFileLines)
            {
                var match = re.Match(line);

                if (match.Success)
                {
                    return match.Groups["Path"].Value;
                }
            }
            throw new ConfigParsingException();
        }
    }

}