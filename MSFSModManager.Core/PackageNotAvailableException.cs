using System;

namespace MSFSModManager.Core
{
    public class PackageNotAvailableException : Exception
    {
        public string PackageId { get; }

        public PackageNotAvailableException(string packageId)
            : base($"Package {packageId} is not available.")
        {
            PackageId = packageId;
        }
    }
}
