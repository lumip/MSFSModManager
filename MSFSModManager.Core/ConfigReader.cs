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