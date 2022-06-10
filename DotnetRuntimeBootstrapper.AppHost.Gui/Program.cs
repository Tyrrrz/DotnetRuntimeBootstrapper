using System;

namespace DotnetRuntimeBootstrapper.AppHost.Gui;

public static class Program
{
    [STAThread]
    public static int Main(string[] args) => new ApplicationShell().Run(args);
}