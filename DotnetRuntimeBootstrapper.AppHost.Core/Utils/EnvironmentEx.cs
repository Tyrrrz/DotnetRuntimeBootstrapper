using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils;

internal static class EnvironmentEx
{
    public static string ProcessPath { get; } = Assembly.GetExecutingAssembly().Location;

    public static void RefreshEnvironmentVariables()
    {
        var machineEnvironmentVariables = Environment
            .GetEnvironmentVariables(EnvironmentVariableTarget.Machine)
            .Cast<DictionaryEntry>();

        foreach (var environmentVariable in machineEnvironmentVariables)
        {
            var key = (string) environmentVariable.Key;
            var value = (string?) environmentVariable.Value;

            Environment.SetEnvironmentVariable(key, value);
        }
    }
}