using System;
using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class PathExtensions
{
    extension(Path)
    {
        public static string GenerateTempFilePath(string fileNameBase)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameBase);
            var fileExtension = Path.GetExtension(fileNameBase);
            var salt = Random.Shared.GetHexString(8, true);

            return Path.Combine(
                Path.GetTempPath(),
                fileNameWithoutExtension + '.' + salt + fileExtension
            );
        }
    }
}
