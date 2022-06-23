namespace DotnetRuntimeBootstrapper.AppHost.Cli;

public static class Program
{
    public static int Main(string[] args) => new Shell().Run(args);
}