using DotnetRuntimeBootstrapper.AppHost.Utils;

namespace DotnetRuntimeBootstrapper.AppHost.Prerequisites
{
    internal class WindowsUpdatePrerequisiteInstaller : IPrerequisiteInstaller
    {
        public IPrerequisite Prerequisite { get; }

        public string FilePath { get; }

        public WindowsUpdatePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
        {
            Prerequisite = prerequisite;
            FilePath = filePath;
        }

        public void Run() => CommandLine.Run("wusa", new[] {FilePath, "/quiet", "/norestart"}, true);
    }
}