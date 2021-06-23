using System;
using System.Globalization;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    public partial class SemanticVersion
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public string ToString(int components) => components switch
        {
            1 => $"{Major}",
            2 => $"{Major}.{Minor}",
            3 => $"{Major}.{Minor}.{Patch}",
            _ => throw new ArgumentOutOfRangeException(nameof(components))
        };

        public override string ToString() => ToString(3);
    }

    public partial class SemanticVersion
    {
        public static SemanticVersion? TryParse(string value)
        {
            var components = value.Split('.');

            // Major
            if (!int.TryParse(
                components.ElementAtOrDefault(0),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var major))
            {
                // Major is required
                return null;
            }

            // Minor
            if (!int.TryParse(
                components.ElementAtOrDefault(1),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var minor))
            {
                // Minor is optional
                minor = 0;
            }

            // Patch
            if (!int.TryParse(
                components.ElementAtOrDefault(2),
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var patch))
            {
                // Patch is optional
                patch = 0;
            }

            return new SemanticVersion(major, minor, patch);
        }

        public static SemanticVersion Parse(string value) =>
            TryParse(value) ??
            throw new FormatException($"String '{value}' does not represent a valid semantic version.");
    }
}