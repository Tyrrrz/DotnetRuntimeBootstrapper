using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotnetRuntimeBootstrapper.Launcher.Utils
{
    internal static class FileEx
    {
        private static readonly Random Random = new();

        private static string GetRandomSuffix()
        {
            var buffer = new StringBuilder(8);

            for (var i = 0; i < 8; i++)
                buffer.Append(Random.Next(0, 10).ToString(CultureInfo.InvariantCulture));

            return buffer.ToString();
        }

        public static string GetTempFileName(string namePrefix, string extension = "tmp") =>
            Path.Combine(
                Path.GetTempPath(),
                namePrefix + '_' + GetRandomSuffix() + '.' + extension.Trim('.')
            );

        public static void TryDelete(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore
            }
        }
    }
}