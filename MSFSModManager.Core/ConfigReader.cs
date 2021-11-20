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


        //         const steamMsfsPath = app.getPath('appData') + "\\Microsoft Flight Simulator\\UserCfg.opt";
        // const msStoreMsfsPath = app.getPath('home') + "\\AppData\\Local\\Packages\\Microsoft.FlightSimulator_8wekyb3d8bbwe\\LocalCache\\UserCfg.opt";

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