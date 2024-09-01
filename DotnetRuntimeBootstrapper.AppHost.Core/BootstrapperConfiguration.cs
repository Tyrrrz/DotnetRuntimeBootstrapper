using System;
using System.Collections.Generic;
using System.Reflection;
using DotnetRuntimeBootstrapper.AppHost.Core.Utils.Extensions;

namespace DotnetRuntimeBootstrapper.AppHost.Core;

public partial class BootstrapperConfiguration
{
    public required string TargetFileName { get; init; }

    public required bool IsPromptRequired { get; init; }
}

public partial class BootstrapperConfiguration
{
    public static BootstrapperConfiguration Resolve()
    {
        var data = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceString(nameof(BootstrapperConfiguration));
        var parsed = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in data.Split('\n'))
        {
            var components = line.Split('=');
            if (components.Length != 2)
                continue;

            var key = components[0].Trim();
            var value = components[1].Trim();

            parsed[key] = value;
        }

        return new BootstrapperConfiguration
        {
            TargetFileName = parsed[nameof(TargetFileName)],
            IsPromptRequired = string.Equals(
                parsed[nameof(IsPromptRequired)],
                "true",
                StringComparison.OrdinalIgnoreCase
            ),
        };
    }
}
