using System.IO;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class FileExtensions
{
    extension(File)
    {
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
