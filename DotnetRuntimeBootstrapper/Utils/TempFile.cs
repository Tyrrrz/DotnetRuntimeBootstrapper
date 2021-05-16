using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DotnetRuntimeBootstrapper.Utils
{
    internal partial class TempFile : IDisposable
    {
        public string Path { get; }

        public TempFile(string path) => Path = path;

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch
            {
                // We tried
            }
        }
    }

    internal partial class TempFile
    {
        private static readonly Random Random = new Random();

        private static string GetRandomSuffix()
        {
            var buffer = new StringBuilder(8);

            for (var i = 0; i < 8; i++)
                buffer.Append(Random.Next(0, 10).ToString(CultureInfo.InvariantCulture));

            return buffer.ToString();
        }

        public static TempFile Create(string namePrefix, string extension) => new TempFile(
            System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                namePrefix + '_' + GetRandomSuffix() + '.' + extension.Trim('.')
            )
        );
    }
}