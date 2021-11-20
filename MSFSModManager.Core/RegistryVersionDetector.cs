using System;
using System.Linq;
using Microsoft.Win32;

namespace MSFSModManager.Core
{

    public interface IGameVersionDetector
    {
        VersionNumber Version { get; }
    }

    public class RegistryVersionDetector : IGameVersionDetector
    {
        public VersionNumber Version { get; }

        private static string REGISTRY_KEY = "Software\\Microsoft\\GamingServices\\PackageRepository\\Package";

        public RegistryVersionDetector()
        {
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (RegistryKey? gamePackagesKey = hklm.OpenSubKey(REGISTRY_KEY))
                {
                    if (gamePackagesKey != null)
                    {
                        VersionNumber[] versionNumber = gamePackagesKey.GetValueNames()
                            .Where(valueName => valueName.Contains("FlightSimulator"))
                            .Select(valueName => {
                                string[] splits = valueName.Split('_');
                                if (splits.Length < 2 || splits[0] != "Microsoft.FlightSimulator")
                                {
                                    throw new Exception("Failed to parse FlightSimulator package registry value.");
                                }
                                return VersionNumber.FromString(splits[1]);
                            }).ToArray();
                        if (versionNumber.Length == 0) throw new Exception("No version number found in registry.");
                        Version = versionNumber[0];
                    }
                    else throw new Exception("could not open package registry key.");
                }
            }
        }
    }

}
