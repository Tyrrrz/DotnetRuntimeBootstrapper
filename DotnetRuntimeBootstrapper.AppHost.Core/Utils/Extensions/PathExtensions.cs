using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class PathExtensions
{
    private static readonly Random Random = new();

    extension(Path)
    {
        public static string Combine(params IEnumerable<string> paths) =>
            paths.Aggregate(Path.Combine);

        public static string GenerateTempFilePath(string fileNameBase)
        {
            static string GenerateSalt()
            {
                var buffer = new StringBuilder(8);

                for (var i = 0; i < 8; i++)
                    buffer.Append(Random.Next(0, 10).ToString(CultureInfo.InvariantCulture));

                return buffer.ToString();
            }

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameBase);
            var fileExtension = Path.GetExtension(fileNameBase);
            var salt = GenerateSalt();

            return Path.Combine(
                Path.GetTempPath(),
                fileNameWithoutExtension + '.' + salt + fileExtension
            );
        }
    }
}
