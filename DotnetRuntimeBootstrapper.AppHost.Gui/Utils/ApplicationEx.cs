using System.Windows.Forms;

namespace DotnetRuntimeBootstrapper.AppHost.Gui.Utils;

internal static class ApplicationEx
{
    private static bool _isInitialized;

    public static void EnsureInitialized()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
    }
}