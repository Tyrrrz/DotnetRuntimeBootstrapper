using System;
using System.Reflection;
using PowerKit.Extensions;
using QuickJson;

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

        var json =
            Json.TryParse(data)
            ?? throw new InvalidOperationException(
                "Failed to parse bootstrapper configuration. Ensure the embedded resource contains valid JSON."
            );

        var targetFileName =
            json.TryGetChild(nameof(TargetFileName))?.TryGetString()
            ?? throw new InvalidOperationException(
                $"Failed to read '{nameof(TargetFileName)}' from bootstrapper configuration."
            );

        var isPromptRequired =
            json.TryGetChild(nameof(IsPromptRequired))?.TryGetBool()
            ?? throw new InvalidOperationException(
                $"Failed to read '{nameof(IsPromptRequired)}' from bootstrapper configuration."
            );

        return new BootstrapperConfiguration
        {
            TargetFileName = targetFileName,
            IsPromptRequired = isPromptRequired,
        };
    }
}
