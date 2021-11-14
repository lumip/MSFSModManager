using System;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace MSFSModManager.Core
{

    public interface IVersionNumber : IComparable<IVersionNumber>
    {

    }

    public class InfiniteVersionNumber : IVersionNumber
    {
        public int CompareTo(IVersionNumber other)
        {
            if (other is InfiniteVersionNumber) return 0;
            return 1;
        }

        public override string ToString()
        {
            return "∞";
        }
    }

    public class VersionNumber : IVersionNumber
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }

        public static IVersionNumber Infinite = new InfiniteVersionNumber();
        public static VersionNumber Zero = new VersionNumber(0, 0, 0);

        public VersionNumber(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public static VersionNumber FromString(String versionString)
        {
            String pattern = @"^(?:[Vv]\s*)?(?:\.)?(?<Major>\d+)(?:\.(?<Minor>\d+)(?:\.(?<Patch>\d+)(?:\.(?<Build>\d+))?(?:-.+)?)?)?$";
            var regex = new Regex(pattern);
            var match = regex.Match(versionString);
            if (!match.Success) throw new FormatException($"Version string '{versionString}' was not correctly formatted.");


            Debug.Assert(match.Groups["Major"].Success);
            int major = int.Parse(match.Groups["Major"].ToString());

            int minor = 0;
            if (match.Groups["Minor"].Success)
            {
                minor = int.Parse(match.Groups["Minor"].ToString());
            }

            int patch = 0;
            if (match.Groups["Patch"].Success)
            {
                patch = int.Parse(match.Groups["Patch"].ToString());
            }
            
            return new VersionNumber(major, minor, patch);
        }

        public int CompareTo(VersionNumber other)
        {
            if (Major > other.Major) return 1;
            if (Major == other.Major)
            {
                if (Minor > other.Minor) return 1;
                if (Minor == other.Minor)
                {
                    if (Patch > other.Patch) return 1;
                    if (Patch == other.Patch) return 0;
                }
            }
            return -1;
        }

        public int CompareTo(IVersionNumber other)
        {
            if (other is VersionNumber) return CompareTo((VersionNumber)other);
            return -other.CompareTo(this);
        }

        public static bool operator<(VersionNumber a, IVersionNumber b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator<(IVersionNumber a, VersionNumber b)
        {
            return a.CompareTo(b) < 0;
        }

        public static bool operator>(VersionNumber a, IVersionNumber b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator>(IVersionNumber a, VersionNumber b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator<=(VersionNumber a, IVersionNumber b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator<=(IVersionNumber a, VersionNumber b)
        {
            return a.CompareTo(b) <= 0;
        }

        public static bool operator>=(VersionNumber a, IVersionNumber b)
        {
            return a.CompareTo(b) >= 0;
        }

        public static bool operator>=(IVersionNumber a, VersionNumber b)
        {
            return a.CompareTo(b) >= 0;
        }

        public override bool Equals(object obj)
        {
            VersionNumber? other = obj as VersionNumber;
            if (other == null) return false;
            return CompareTo(other) == 0;
        }

        public override String ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        public override int GetHashCode()
        {
            return (Major << 16) | (Minor << 8) | Patch;
        }

        public static IVersionNumber Min(IVersionNumber a, IVersionNumber b)
        {
            return (a.CompareTo(b) <= 0) ? a : b;
        }

        public static IVersionNumber Max(IVersionNumber a, IVersionNumber b)
        {
            return (a.CompareTo(b) >= 0) ? a : b;
        }

        public VersionNumber Increment()
        {
            return new VersionNumber(Major, Minor, Patch + 1);
        }

        public VersionNumber IncrementMinor()
        {
            return new VersionNumber(Major, Minor + 1, 0);
        }

        public VersionNumber IncrementMajor()
        {
            return new VersionNumber(Major + 1, 0, 0);
        }
    }

}
