namespace DotnetRuntimeBootstrapper.Executable.RuntimeComponents
{
    public interface IRuntimeComponentInstaller
    {
        IRuntimeComponent Component { get; }

        string FilePath { get; }

        void Run();
    }
}