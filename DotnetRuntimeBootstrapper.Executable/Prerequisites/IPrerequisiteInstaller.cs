namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public interface IPrerequisiteInstaller
    {
        IPrerequisite Prerequisite { get; }

        string FilePath { get; }

        void Run();
    }
}