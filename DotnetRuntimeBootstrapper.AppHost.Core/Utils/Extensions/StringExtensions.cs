namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class StringExtensions
{
    extension(string str)
    {
        public string? NullIfEmptyOrWhiteSpace() => !string.IsNullOrWhiteSpace(str) ? str : null;
    }
}
