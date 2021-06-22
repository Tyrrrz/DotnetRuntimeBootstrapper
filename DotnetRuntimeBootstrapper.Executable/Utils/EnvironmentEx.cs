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
                .Cast<DictionaryEntry>()
                .ToDictionary(e => (string) e.Key, e => (string?) e.Value);

            foreach (var environmentVariable in machineEnvironmentVariables)
            {
                Environment.SetEnvironmentVariable(environmentVariable.Key, environmentVariable.Value);
            }
        }
    }
}