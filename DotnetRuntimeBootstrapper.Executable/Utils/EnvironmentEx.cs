using System;
using System.Collections;
using System.Linq;

namespace DotnetRuntimeBootstrapper.Executable.Utils
{
    internal static class EnvironmentEx
    {
        public static void ResetEnvironmentVariables()
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
}