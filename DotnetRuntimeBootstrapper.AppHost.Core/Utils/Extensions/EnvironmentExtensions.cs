using System;
using System.Collections;
using System.Linq;

namespace DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

internal static class EnvironmentExtensions
{
    extension(Environment)
    {
        public static void RefreshEnvironmentVariables()
        {
            var machineEnvironmentVariables = Environment
                .GetEnvironmentVariables(EnvironmentVariableTarget.Machine)
                .Cast<DictionaryEntry>();

            foreach (var environmentVariable in machineEnvironmentVariables)
            {
                var key = (string)environmentVariable.Key;
                var value = (string?)environmentVariable.Value;

                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
}
