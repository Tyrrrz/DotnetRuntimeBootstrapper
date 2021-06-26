using System.Diagnostics;

namespace DotnetRuntimeBootstrapper.Executable.Prerequisites
{
    public class ExecutablePrerequisiteInstaller : IPrerequisiteInstaller
    {
        public IPrerequisite Prerequisite { get; }

        public string FilePath { get; }

        public ExecutablePrerequisiteInstaller(IPrerequisite prerequisite, string filePath)
        {
            Prerequisite = prerequisite;
            FilePath = filePath;
        }

        public void Run()
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FilePath,
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}