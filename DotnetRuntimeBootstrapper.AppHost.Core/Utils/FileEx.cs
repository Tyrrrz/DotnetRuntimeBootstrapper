using System.Globalization;
using System.IO;
using System.Text;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class FileEx
{
    private static string GenerateSalt()
    {
        var buffer = new StringBuilder(8);

        for (var i = 0; i < 8; i++)
            buffer.Append(RandomEx.Instance.Next(0, 10).ToString(CultureInfo.InvariantCulture));

        return buffer.ToString();
    }

    public static string GenerateTempFilePath(string fileNameBase)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileNameBase);
        var fileExtension = Path.GetExtension(fileNameBase);
        var salt = GenerateSalt();

        return Path.Combine(
            Path.GetTempPath(),
            fileNameWithoutExtension + '.' + salt + fileExtension
        );
    }

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